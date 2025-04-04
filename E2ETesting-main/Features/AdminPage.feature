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

@Admin
@AdminPage
@AdminLogin
Scenario: Switching between Hantera Användare och Hantera Recensioner
	Given I am on the Admin Page
	And I see the Hantera Användare List
	When I click the Hantera Recensioner button
	Then I should see the Recensioner List

@DeleteReviewAdmin
Scenario: Deleting a Review in the Hantera Recensioner list as Admin
	Given I am on the Admin Page
	And I see the Recensioners List
	When I click the delete button on the latest review added
	Then the Review should be removed





