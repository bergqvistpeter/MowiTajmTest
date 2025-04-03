Feature: Admidotnet testnPageSteps

Tester på vår AdminPage

@Admin
@LoginPage
Scenario: Login as Admin
	Given I am on the login page
	When I enter "peter.bergqvist@edu.newton.se" as the username
	And I enter "Peter_123" as the password
	And I click the logga in button button
	Then I should be Logged in
	And be redirected to Index page
	And I should see the Admin button


@Admin
@IndexPage
@AdminLogin
Scenario: Clicking the Admin button
	Given I am on the Index Page
	And I am logged in as Admin
	When I click the Admin button
	Then I should be redirected to the Admin page


@ignore
Scenario: See the Admin Button
	Given [context]
	When [action]
	Then [outcome]
