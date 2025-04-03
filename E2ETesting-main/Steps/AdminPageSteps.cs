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

    }

}
