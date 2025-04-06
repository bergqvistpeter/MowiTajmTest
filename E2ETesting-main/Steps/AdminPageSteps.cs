using System;
using TechTalk.SpecFlow;
using Microsoft.Playwright;
using Microsoft.Identity.Client;
using Microsoft.JSInterop.Infrastructure;
using E2ETesting.Hooks;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;


namespace E2ETesting.Steps
{
    [Binding]
    public class AdminPageSteps(ScenarioContext scenarioContext) : BaseSteps(scenarioContext)
    {

        //Scenario: Logga in som Admin
        //Om jag är på Loginsidan
        [Given(@"I am on the login page")]
        public async Task GivenIAmOnTheLoginPage()
        {
            _page = HooksSetup.pageObject;
            await _page.GotoAsync("https://localhost:7295/Identity/Account/Login");
        }

        //Jag skriver in Admin Username
        [When(@"I enter ""([^""]*)"" as the username")]
        public async Task WhenIEnterAsTheUsername(string username)
        {
            await _page.FillAsync("input[name='Input.Email']", username);
        }
        //Jag skriver in Admin Lösenord
        [When(@"I enter ""(.*)"" as the password")]
        public async Task WhenIEnterAsThePassword(string password)
        {
            await _page.FillAsync("input[name='Input.Password']", password);
        }
        // Jag klickar på logga in knappen
        [When(@"I click the logga in button")]
        public async Task WhenIClickTheLoggaInButtonButton()
        {
            await _page.ClickAsync("#login-submit");
        }


        // Då ska jag  se Admin knappen
        [Then(@"I should see the Admin button")]
        public async Task ThenIShouldSeeTheAdminButton()
        {
            var isVisible = await _page.IsVisibleAsync("a[href='/Identity/Admin']");
            Assert.True(isVisible, "Admin-knappen är inte synlig på sidan.");
        }

        [Then(@"I should be Logged in")]
        public async Task ThenIShouldBeLoggedIn()
        {
            await _page.WaitForURLAsync("https://localhost:7295/Movies");
            // Kontrollera att jag är inloggad
            var loggedIn = await _page.Locator("text=Logga ut").IsVisibleAsync();
        }

        [Then(@"be redirected to Index page")]
        public async Task ThenBeRedirectedToIndexPage()
        {
            await _page.WaitForURLAsync("**/Movies");
            Assert.Contains("/Movies", _page.Url);
        }

        //Scenario: Clicking the Admin button

        // Om jag är på Index sidan
        [Given(@"I am on the Index Page")]
        public async Task GivenIAmOnTheIndexPage()
        {
            _page = HooksSetup.pageObject;
            await _page.GotoAsync("https://localhost:7295/Movies");

        }

        //Och jag är inloggad som Admin
        [Given(@"I am logged in as Admin")]
        public async Task GivenIAmLoggedInAsAdmin()
        {
            var isVisible = await _page.IsVisibleAsync("a[href='/Identity/Admin']");
            Assert.True(isVisible, "Admin-knappen är inte synlig på sidan.");
        }

        [When(@"I click the Admin button")]
        public async Task WhenIClickTheAdminButton()
        {
            await _page.ClickAsync("a[href='/Identity/Admin']");
        }

        [Then(@"I should be redirected to the Admin page")]
        public async Task ThenIShouldBeRedirectedToTheAdminPage()
        {
            await _page.WaitForURLAsync("**/Admin");
            Assert.Contains("/Admin", _page.Url);
        }

        //Scenario: Switching between Hantera Användare och Hantera Recensioner

        //Om jag är på Admin sidan
        [Given(@"I am on the Admin Page")]
        public async Task GivenIAmOnTheAdminPage()
        {
            _page = HooksSetup.pageObject;
            await _page.GotoAsync("https://localhost:7295/Identity/Admin");
            Assert.Contains("/Admin", _page.Url);

        }

        //Och jag ser Hantera Användare Listan
        [Given(@"I see the Hantera Användare List")]
        public async Task GivenISeeTheHanteraAnvandareList()
        {
            // Vänta tills knappen är synlig
            var button = await _page.WaitForSelectorAsync("#LoadUsers");

            // Kontrollera att knappen har klassen "active"
            bool isActive = await button.EvaluateAsync<bool>("button => button.classList.contains('active')");

            Assert.True(isActive, "Knappen är inte aktiv.");
        }

        //När jag trycker på Hantera Recensioner
        [When(@"I click the Hantera Recensioner button")]
        public async Task WhenIClickTheHanteraRecensionerButton()
        {
            await _page.ClickAsync("#LoadReviews");
        }

        [Then(@"I should see the Recensioner List")]
        public async Task ThenIShouldSeeTheRecensionerList()
        {
            // Vänta tills knappen är synlig
            var button = await _page.WaitForSelectorAsync("#LoadReviews");

            // Kontrollera att knappen har klassen "active"
            bool isActive = await button.EvaluateAsync<bool>("button => button.classList.contains('active')");

            Assert.True(isActive, "Knappen är inte aktiv.");
        }


        //Scenario: Deleting a Review in the Hantera Recensioner list as Admin

        [Given(@"I see the Recensioners List")]
        public async Task GivenISeeTheRecensionersList()
        {
            await _page.ClickAsync("#LoadReviews");
            // Vänta tills knappen är synlig
            var button = await _page.WaitForSelectorAsync("#LoadReviews");

            // Kontrollera att knappen har klassen "active"
            bool isActive = await button.EvaluateAsync<bool>("button => button.classList.contains('active')");
            Assert.NotNull(button);
            Assert.True(isActive, "Knappen är inte aktiv.");
            
        }

        [When(@"I click the delete button on the latest review added")]
        public async Task WhenIClickTheDeleteButtonOnTheLatestReviewAdded()
        {
            // Hämta alla recensioner (tr-taggar) på sidan
            var reviews = await _page.QuerySelectorAllAsync("tr.review-row");

            // Hämta och konvertera datum för varje recension
            var reviewData = new List<(IElementHandle Review, DateTime Date)>();

            foreach (var review in reviews)
            {
                var dateElement = await review.QuerySelectorAsync("td:nth-child(6)"); // Kolumnen med datum
                if (dateElement != null)
                {
                    var dateText = await dateElement.GetPropertyAsync("textContent");
                    if (dateText != null)
                    {
                        if (DateTime.TryParse(dateText.ToString().Trim(), out DateTime parsedDate))
                        {
                            reviewData.Add((review, parsedDate));
                        }
                    }
                }
            }

            // Sortera recensionerna efter datum i fallande ordning (senaste först)
            var latestReview = reviewData.OrderByDescending(r => r.Date).FirstOrDefault().Review;

            if (latestReview != null)
            {
                // Hitta delete-knappen på samma rad som den senaste recensionen
                var deleteButton = await latestReview.QuerySelectorAsync("button[title='Ta bort recension']");
                if (deleteButton != null)
                {
                    // Klicka på delete-knappen
                    await deleteButton.ClickAsync();

                    // Eventuellt hantera dialogen om bekräftelse
                    _page.Dialog += async (_, dialog) =>
                    {
                        if (dialog.Type == "confirm")
                        {
                            await dialog.AcceptAsync(); // Klicka på "OK" för att acceptera dialogen
                        }
                    };
                }
                else
                {
                    throw new Exception("Delete-knappen hittades inte.");
                }
            }
            else
            {
                throw new Exception("Ingen recension hittades.");
            }
        }



        [Then(@"the Review should be removed")]
        public async Task ThenTheReviewShouldBeRemoved()
        {
            // Hämta alla recensioner på sidan
            var reviews = await _page.QuerySelectorAllAsync("tr.review-row");

            // Definiera de specifika värdena från den recension som vi just tog bort
            string deletedMovieName = "Kopps";  // Filmnamn på den borttagna recensionen
            string deletedUsername = "Bqvist";  // Användarnamn på den borttagna recensionen
            string deletedDate = DateTime.Now.ToString("yyyy-MM-dd");  // Använd dagens datum (t.ex. "2025-04-04")

            bool reviewFound = false;

            // Hitta den borttagna recensionen genom att matcha filmnamn, användarnamn och datum
            foreach (var review in reviews)
            {
                // Hämta filmnamnet, användarnamnet och datumet för recensionen
                var movieName = await review.QuerySelectorAsync("td.movie-column a");
                var username = await review.QuerySelectorAsync("td:nth-child(5)"); // Användarnamn på kolumn 5 (justera vid behov)
                var reviewDate = await review.QuerySelectorAsync("td:nth-child(6)"); // Datum på kolumn 6 (justera vid behov)

                var movieText = await movieName.InnerTextAsync();
                var usernameText = await username.InnerTextAsync();
                var dateText = await reviewDate.InnerTextAsync();

                // Kontrollera om filmnamn, användarnamn och datum matchar den borttagna recensionen
                if (movieText.Contains(deletedMovieName) && usernameText.Contains(deletedUsername) && dateText.Contains(deletedDate))
                {
                    reviewFound = true;
                    break;
                }
            }

            // Kontrollera att recensionen inte finns kvar
            Assert.False(reviewFound, "Recensionen finns fortfarande kvar i listan.");
        }

        //Scenario Outline: Att sortera recensioner efter betyg på Hantera Recensioner Sidan

        //När jag väljer ett betyg från betygsfilter

        [When(@"I select ""([^""]*)"" from the rating filter")]
        public async Task WhenISelectFromTheRatingFilter(string rating)
        {
            // 🔹 Vänta på att dropdown-menyn blir synlig
            await _page.WaitForSelectorAsync("#ratingFilter");

            // 🔹 Välj betyget från dropdown-menyn
            await _page.SelectOptionAsync("#ratingFilter", new SelectOptionValue { Label = rating });

            // 🔹 Vänta en kort stund så att filtreringen kan uppdateras
            await _page.WaitForTimeoutAsync(1000);
        }

        // då ska jag se recensioner med det valda betyget i Hantera Recensioner listan

        [Then(@"^I should see reviews with ""(.*)""$")]
        public async Task ThenIShouldSeeReviewsWith(string expectedRating)
        {
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle); // Vänta tills filtret är klart

            // Hämta alla recensioner
            var allReviews = await _page.QuerySelectorAllAsync("tr.review-row");

            // Filtrera ut synliga recensioner
            var visibleReviews = new List<IElementHandle>();
            foreach (var review in allReviews)
            {
                if (await review.IsVisibleAsync()) // Kontrollera om recensionen är synlig
                {
                    visibleReviews.Add(review);
                }
            }

            Assert.True(visibleReviews.Any(), "❌ Inga synliga recensioner hittades efter filtrering.");

            // Hämta betygen för de synliga recensionerna
            var ratings = new List<string>();
            foreach (var review in visibleReviews)
            {
                var ratingElement = await review.QuerySelectorAsync("td:nth-child(4)");
                if (ratingElement != null)
                {
                    var rating = await ratingElement.InnerTextAsync();
                    ratings.Add(rating.Trim());
                }
            }

            // Kontrollera om alla betyg matchar det förväntade betyget
            bool allRatingsMatch = ratings.All(rating => rating == expectedRating);
            Assert.True(allRatingsMatch, $"❌ Felaktiga recensioner hittades: Förväntat '{expectedRating}', men vissa hade andra betyg: [{string.Join(", ", ratings)}]");

            Console.WriteLine("Alla synliga recensioner har korrekt betyg!");
        }


        //Scenario Outline: Att sortera recensioner efter film

        [When(@"I select ""([^""]*)"" from the movie filter")]
        public async Task WhenISelectFromTheMovieFilter(string movie)
        {
            // 🔹 Vänta på att dropdown-menyn blir synlig
            await _page.WaitForSelectorAsync("#movieFilter");

            // 🔹 Välj betyget från dropdown-menyn
            await _page.SelectOptionAsync("#movieFilter", new SelectOptionValue { Label = movie });

            // 🔹 Vänta en kort stund så att filtreringen kan uppdateras
            await _page.WaitForTimeoutAsync(1000);
        }

        [Then(@"I should see the reviews matching the movie ""([^""]*)""")]
        public async Task ThenIShouldSeeTheReviewsMatchingTheMovie(string expectedMovie)
        {
            // Vänta tills recensionerna har laddats och synliga
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var allReviews = await _page.QuerySelectorAllAsync("tr.review-row"); // Hämta alla recensioner
            var visibleReviews = new List<IElementHandle>();

            // Filtrera synliga recensioner
            foreach (var review in allReviews)
            {
                if (await review.IsVisibleAsync()) // Kontrollera om recensionen är synlig
                {
                    visibleReviews.Add(review);
                }
            }

            // Kontrollera att vi har några synliga recensioner
            Assert.True(visibleReviews.Any(), "❌ Inga synliga recensioner hittades.");

            // Hämta filmtiteln från de synliga recensionerna
            var movieTitles = new List<string>();
            foreach (var review in visibleReviews)
            {
                var titleElement = await review.QuerySelectorAsync("td:nth-child(1)"); // Anta att filmen är i kolumn 1
                if (titleElement != null)
                {
                    var movieTitle = await titleElement.InnerTextAsync();
                    movieTitles.Add(movieTitle.Trim());
                }
            }

            // Kontrollera att alla recensioner matchar den förväntade filmens titel
            bool allTitlesMatch = movieTitles.All(title => title.Contains(expectedMovie));
            Assert.True(allTitlesMatch, $"❌ Felaktiga recensioner hittades: Förväntat film '{expectedMovie}', men vissa recensioner hade andra filmer: [{string.Join(", ", movieTitles)}]");

            Console.WriteLine("Alla synliga recensioner har korrekt filmtitel!");
        }

    }

}
