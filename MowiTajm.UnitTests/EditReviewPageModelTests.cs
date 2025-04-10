using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MowiTajm.Data;
using MowiTajm.Models;
using MowiTajm.Pages.Movies;
using System.Threading.Tasks;
using Xunit;

namespace MowiTajm.Tests
{
    public class EditReviewPageModelTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IUserService> _mockUserService;
        private readonly EditReviewPageModel _pageModel;

        // Konstruktor för testklassen
        public EditReviewPageModelTests()
        {
            // Skapar en In-Memory DbContext som används i testerna
            // `UseInMemoryDatabase` skapar en databas som lagras i minnet och inte på disk.
            // Vi använder ett unikt namn för varje test med hjälp av ett GUID så att vi får en separat databas per test.
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid()) // Genererar ett unikt namn för varje test
                .Options;

            // Skapar en instans av ApplicationDbContext med in-memory-databasen
            _dbContext = new ApplicationDbContext(options);

            // Mockar IUserService. Detta gör att vi kan simulera användartjänstens beteende i tester.
            _mockUserService = new Mock<IUserService>();

            // Här mockar vi `GetUserContextAsync`-metoden i IUserService för att returnera ett mockat användarkontext.
            // Detta kan vara användbart om din kod interagerar med användartjänsten.
            _mockUserService.Setup(service => service.GetUserContextAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(new UserContext { DisplayName = "Test User" });

            // Skapar en instans av EditReviewPageModel som vi ska testa.
            // Den tar ApplicationDbContext och IUserService som beroenden.
            _pageModel = new EditReviewPageModel(_dbContext, _mockUserService.Object);
        }

        // Testmetod som kör OnPostAsync när recensionen är korrekt och uppdaterar databasen
        [Fact]
        public async Task OnPostAsync_UpdatesReview_WhenModelIsValid()
        {
            // ARRANGERA (Setup) - Skapa en testrecension som ska läggas till i databasen
            var review = new Review { Id = 1, ImdbID = "tt1234567", Text = "Great movie!" };

            // Lägg till recensionen i den in-memory databasen
            _dbContext.Reviews.Add(review);
            await _dbContext.SaveChangesAsync();  // Spara objektet till databasen så att det finns i databasen för nästa steg

            // Tilldela den skapade recensionen till Review-egenskapen i PageModel
            // Detta simulerar att användaren redigerar en recension på sidan
            _pageModel.Review = review;

            // HANDLING (Act) - Kör OnPostAsync, vilket simulerar att formuläret postas och att recensionen uppdateras.
            var result = await _pageModel.OnPostAsync();

            // VERIFIERA (Assert) - Verifiera resultatet av OnPostAsync-metoden
            // Här kollar vi om resultatet är en omdirigering till en annan sida
            Assert.IsType<RedirectToPageResult>(result);  // Vi förväntar oss en omdirigering till en annan sida

            // Fånga den omdirigerade sidan och kontrollera att vi går till rätt sida
            var redirectResult = result as RedirectToPageResult;
            Assert.Equal("MovieDetailsPage", redirectResult.PageName);  // Förväntar oss att sidan vi omdirigeras till heter "MovieDetailsPage"
            Assert.Equal(review.ImdbID, redirectResult.RouteValues["imdbID"]);  // Vi förväntar oss att imdbID är med i route-värdena för omdirigeringen

            // Slutligen, verifiera att recensionen faktiskt har uppdaterats i databasen.
            // Hämta recensionen från databasen igen och kontrollera att den har sparats korrekt
            var updatedReview = await _dbContext.Reviews.FindAsync(review.Id);
            Assert.Equal(review.Text, updatedReview.Text);  // Kontrollera att texten på recensionen har sparats korrekt
        }
    }
}

