Feature: cache provider

  @prod
  Scenario: able to store and retrieve update
    Given cache provider "Hybrid" is registered
    When I store update to cache
    Then I should be able to retrieve update from cache