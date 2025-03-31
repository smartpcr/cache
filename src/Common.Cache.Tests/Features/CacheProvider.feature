Feature: distributed cache

  @prod
  Scenario: simple memory cache scenario
    Given cache provider "<CacheProvider>" is registered
    Given store a cached item with ttl of 5 minutes
      | Key   | Size   |
      | <key> | <size> |
    Then I can validate the cached item
      | Key   | Size   |
      | <key> | <size> |
    And cached item should still be valid after 4 minutes
      | Key   | Size   |
      | <key> | <size> |
    And cached item should be expired after 2 minutes
      | Key   | Size   |
      | <key> | <size> |

  Examples:
    | CacheProvider | key   | size  |
    | Memory        | key_1 | 0     |
    | Memory        | key_2 | 128   |
    | Memory        | key_3 | 1024  |
    | Memory        | key_4 | 16348 |
    | Csv           | key_1 | 0     |
    | Csv           | key_2 | 128   |
    | Csv           | key_3 | 1024  |
    | Csv           | key_4 | 16348 |
    | Hybrid        | key_1 | 0     |
    | Hybrid        | key_2 | 128   |
    | Hybrid        | key_3 | 1024  |
    | Hybrid        | key_4 | 16348 |
