Feature: buffer release

  As a user,
  I want to be able to release buffer,
  so that I can free up memory.

  Background:
    Given I have a hybrid cache provider
    And I have a buffer release settings
      | BufferReleaseEnabled | BufferReleaseInterval |
      | true                | 1                    |

  Scenario: able to release buffer
    Given a cached item
      | key   | value |
      | key_1 | Fred   |
    When I store cached item
    Then cached item should be added to cache
    When I release buffer
    Then buffer should be released