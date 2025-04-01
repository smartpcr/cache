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
    | CacheProvider | key    | size    |
    | Memory        | key_1  | 0       |
    | Memory        | key_2  | 128     |
    | Memory        | key_3  | 1024    |
    | Memory        | key_4  | 16348   |
    | Memory        | Key_5  | 5242880 |
    | Csv           | key_6  | 0       |
    | Csv           | key_7  | 128     |
    | Csv           | key_8  | 1024    |
    | Csv           | key_9  | 16348   |
    | Csv           | Key_10 | 5242880 |

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

  @prod
  Scenario: windows registry cache scenario
    Given cache provider "WindowsRegistry" is registered
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
    | C021 | 123 | Joe       | Doe      | 1990-01-01 |