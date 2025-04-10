using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MowiTajm.Data;
using MowiTajm.Models;

namespace MowiTajm.Tests
{
    public class IndexModelTests
    {
        // Mockade objekt för UserManager och RoleManager, som hanterar användare och roller
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

        // In-memory-databas för att simulera databasinteraktioner utan att använda en riktig databas
        private readonly ApplicationDbContext _dbContext;
        private readonly IndexModel _indexModel; // Testinstans av IndexModel

        public IndexModelTests()
        {
            // Skapar en in-memory-databas istället för en riktig databas
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase") // Använder en databas endast för testet
                .Options;

            _dbContext = new ApplicationDbContext(options); // Skapar en instans av databaskontexten

            // Mockar UserManager och RoleManager
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null
            );

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null, null, null, null
            );

            // Skapar en testinstans av IndexModel med mockade beroenden och den in-memory-databasen
            _indexModel = new IndexModel(_mockUserManager.Object, _mockRoleManager.Object, _dbContext);
        }

        // Testmetod för att kontrollera om recensioner laddas korrekt
        [Fact]
        public void OnPostLoadReviewsAsync_LaddarRecensioner_Korrekt()
        {
            // Arrange: Lägg till en testrecension i den in-memory databasen
            _dbContext.Reviews.Add(new Review { Id = 1, Text = "Bra!" });
            _dbContext.SaveChanges(); // Sparar ändringar i databasen

            // Act: Kör metoden som laddar recensionerna
            var result = _indexModel.OnPostLoadReviewsAsync();

            // Assert: Kontrollera att recensionerna har laddats korrekt
            Assert.True(_indexModel.ShowReviews); // Kontrollera att ShowReviews sätts till true
            Assert.Single(_indexModel.Reviews);  // Kontrollera att endast en recension laddades
            Assert.IsType<PageResult>(result);   // Kontrollera att metoden returnerar en PageResult
        }

        // Testmetod för att kontrollera om en användares roll uppdateras korrekt
        [Fact]
        public async Task OnPostUpdateRoleAsync_UppdaterarRoll_Korrekt()
        {
            // Arrange: Skapa en testanvändare och simulera rolländring
            var user = new ApplicationUser { Id = "1", UserName = "TestUser" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user); // Mocka att användaren hittas
            _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "OldRole" }); // Mocka nuvarande roll
            _mockUserManager.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success); // Mocka att den gamla rollen tas bort
            _mockUserManager.Setup(x => x.AddToRoleAsync(user, "Admin"))
                .ReturnsAsync(IdentityResult.Success); // Mocka att den nya rollen läggs till

            // Act: Kör metoden för att uppdatera användarens roll
            var result = await _indexModel.OnPostUpdateRoleAsync("1", "Admin");

            // Assert: Kontrollera att användaren omdirigeras efter rolluppdateringen
            Assert.IsType<RedirectToPageResult>(result);
        }

        // Testmetod för att kontrollera att recensioner tas bort korrekt
        [Fact]
        public async Task OnPostDeleteReviewAsync_TarBortRecension_Korrekt()
        {
            // Arrange: Lägg till en testrecension i den in-memory databasen
            var review = new Review { Id = 1, Text = "Bra!" };
            _dbContext.Reviews.Add(review);
            await _dbContext.SaveChangesAsync(); // Sparar ändringar i databasen

            // Act: Kör metoden för att ta bort en recension
            var result = await _indexModel.OnPostDeleteReviewAsync(1);

            // Assert: Kontrollera att metoden returnerar en RedirectToPageResult
            Assert.IsType<RedirectToPageResult>(result);
        }
    }
}
