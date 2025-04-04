using System;
using TechTalk.SpecFlow;
using Microsoft.Playwright;
using Microsoft.Identity.Client;
using Microsoft.JSInterop.Infrastructure;
using E2ETesting.Hooks;

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
        [When(@"I click the logga in button button")]
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
            // Vänta tills knappen är synlig
            var button = await _page.WaitForSelectorAsync("#LoadReviews");

            // Kontrollera att knappen har klassen "active"
            bool isActive = await button.EvaluateAsync<bool>("button => button.classList.contains('active')");

            Assert.True(isActive, "Knappen är inte aktiv.");
        }

        [When(@"I click the delete button on the latest review added")]
        public async Task WhenIClickTheDeleteButtonOnTheLatestReviewAdded()
        {
            // Hitta alla recensioner (tr-taggar) på sidan
            var reviews = await _page.QuerySelectorAllAsync("tr.review-row");

            // Sortera recensionerna efter datum (t.ex. den senaste recensionen är den med det största datumet)
            var latestReview = reviews.OrderByDescending(async review =>
            {
                var dateElement = await review.QuerySelectorAsync("td:nth-child(6)"); // Kolumnen med datum
                var dateText = await dateElement.GetPropertyAsync("textContent"); // Hämta textinnehåll
                return DateTime.Parse(dateText.ToString().Trim()); // Trimma och omvandla texten till DateTime
            }).FirstOrDefault();

            if (latestReview != null)
            {
                // Hitta delete-knappen på samma rad som den senaste recensionen
                var deleteButton = await latestReview.QuerySelectorAsync("button[title='Ta bort recension']");

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

    }

}
