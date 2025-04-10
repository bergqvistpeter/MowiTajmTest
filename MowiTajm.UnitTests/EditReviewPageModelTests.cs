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

        // Konstruktor f�r testklassen
        public EditReviewPageModelTests()
        {
            // Skapar en In-Memory DbContext som anv�nds i testerna
            // `UseInMemoryDatabase` skapar en databas som lagras i minnet och inte p� disk.
            // Vi anv�nder ett unikt namn f�r varje test med hj�lp av ett GUID s� att vi f�r en separat databas per test.
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid()) // Genererar ett unikt namn f�r varje test
                .Options;

            // Skapar en instans av ApplicationDbContext med in-memory-databasen
            _dbContext = new ApplicationDbContext(options);

            // Mockar IUserService. Detta g�r att vi kan simulera anv�ndartj�nstens beteende i tester.
            _mockUserService = new Mock<IUserService>();

            // H�r mockar vi `GetUserContextAsync`-metoden i IUserService f�r att returnera ett mockat anv�ndarkontext.
            // Detta kan vara anv�ndbart om din kod interagerar med anv�ndartj�nsten.
            _mockUserService.Setup(service => service.GetUserContextAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(new UserContext { DisplayName = "Test User" });

            // Skapar en instans av EditReviewPageModel som vi ska testa.
            // Den tar ApplicationDbContext och IUserService som beroenden.
            _pageModel = new EditReviewPageModel(_dbContext, _mockUserService.Object);
        }

        // Testmetod som k�r OnPostAsync n�r recensionen �r korrekt och uppdaterar databasen
        [Fact]
        public async Task OnPostAsync_UpdatesReview_WhenModelIsValid()
        {
            // ARRANGERA (Setup) - Skapa en testrecension som ska l�ggas till i databasen
            var review = new Review { Id = 1, ImdbID = "tt1234567", Text = "Great movie!" };

            // L�gg till recensionen i den in-memory databasen
            _dbContext.Reviews.Add(review);
            await _dbContext.SaveChangesAsync();  // Spara objektet till databasen s� att det finns i databasen f�r n�sta steg

            // Tilldela den skapade recensionen till Review-egenskapen i PageModel
            // Detta simulerar att anv�ndaren redigerar en recension p� sidan
            _pageModel.Review = review;

            // HANDLING (Act) - K�r OnPostAsync, vilket simulerar att formul�ret postas och att recensionen uppdateras.
            var result = await _pageModel.OnPostAsync();

            // VERIFIERA (Assert) - Verifiera resultatet av OnPostAsync-metoden
            // H�r kollar vi om resultatet �r en omdirigering till en annan sida
            Assert.IsType<RedirectToPageResult>(result);  // Vi f�rv�ntar oss en omdirigering till en annan sida

            // F�nga den omdirigerade sidan och kontrollera att vi g�r till r�tt sida
            var redirectResult = result as RedirectToPageResult;
            Assert.Equal("MovieDetailsPage", redirectResult.PageName);  // F�rv�ntar oss att sidan vi omdirigeras till heter "MovieDetailsPage"
            Assert.Equal(review.ImdbID, redirectResult.RouteValues["imdbID"]);  // Vi f�rv�ntar oss att imdbID �r med i route-v�rdena f�r omdirigeringen

            // Slutligen, verifiera att recensionen faktiskt har uppdaterats i databasen.
            // H�mta recensionen fr�n databasen igen och kontrollera att den har sparats korrekt
            var updatedReview = await _dbContext.Reviews.FindAsync(review.Id);
            Assert.Equal(review.Text, updatedReview.Text);  // Kontrollera att texten p� recensionen har sparats korrekt
        }
    }
}

