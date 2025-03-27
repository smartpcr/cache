Feature: dependency injection

As a developer,
I wan to use dependency injection, either service collection or unity,
to register hybridcache,
so that I can use the cache provider in my application.

Scenario: Register hybrid cache provider with service collection
    Given I have a service collection
    When I register hybrid cache provider with service collection
    Then I should be able to resolve the hybrid cache provider

Scenario: Register hybrid cache provider with unity
    Given I have a unity container
    When I register hybrid cache provider with unity container
    Then I should be able to resolve the hybrid cache provider
