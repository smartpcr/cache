Feature: stampede

  @prod
  Scenario: multiple gets share single backend fetch
    Given cache provider "Hybrid" is registered
    Given a customer stored in backend api
      | Key  | Id  | FirstName | LastName | BirthDay   |
      | C001 | 100 | Joe       | Doe      | 1990-01-01 |
    When I try to fetch customer from cache with fetch from api upon cache miss
      | CallCount   | CanBeCanceled   |
      | <CallCount> | <CanBeCanceled> |
    Then expected customer should be returned
      | Key  | Id  | FirstName | LastName | BirthDay   |
      | C001 | 100 | Joe       | Doe      | 1990-01-01 |
    And backend call count should be
      | ExecutionCount |
      | 1              |

  Examples:
    | CallCount | CanBeCanceled |
    | 1         | false         |
    | 1         | true          |
    | 10        | false         |
    | 10        | true          |