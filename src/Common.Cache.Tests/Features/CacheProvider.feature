Feature: cache provider

  @prod
  Scenario: simple cache scenario
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

  @prod
  Scenario: hybrid cache scenario
    Given cache provider "Hybrid" is registered
    Given store a customer with ttl of 5 minutes
      | Key   | Id   | FirstName   | LastName   | BirthDay   |
      | <Key> | <Id> | <FirstName> | <LastName> | <BirthDay> |
    Then I can validate customer
      | Key   | Id   | FirstName   | LastName   | BirthDay   |
      | <Key> | <Id> | <FirstName> | <LastName> | <BirthDay> |
    And cached customer should still be valid after 4 minutes
      | Key   |
      | <Key> |
    And cached customer should be expired after 2 minutes
      | Key   |
      | <Key> |

  Examples:
    | Key  | Id  | FirstName | LastName | BirthDay   |
    | C001 | 100 | Joe       | Doe      | 1990-01-01 |