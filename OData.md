# OData Queries
Most `GET` endpoints provide parameters for sorting and filtering data. The backend uses a subset of the query protocol [OData] (https://learn.microsoft.com/en-us/odata/) for this purpose.

## Basic rules
- Field/column name is not case-sensitive
- Enum values resolved by case-insesitive name

## Supported features
- `top`
- `skip`
- `select` (only on db level)
- `orderby`
- filter
  - `Or`
  - `And`
  - `Equal`
  - `NotEqual`
  - `GreaterThan`
  - `GreaterThanOrEqual`
  - `LessThan`
  - `LessThanOrEqual`
  - group filter terms with `()`
  - supported functions
    - `startswith` (string)
    - `endswith` (string)
    - `contains` (string)
    - `year` (datetime)
    - `month` (datetime)
    - `day` (datetime)
    - `hour` (datetime)
    - `minute` (datetime)
    - `date` (datetime)
    - `time` (datetime)

## Unsupported query features
- `expand`
- `search`
- filter
    - `Add`
    - `Subtract`
    - `Multiply`
    - `Divide`
    - `Modulo`
    - `Has`
  - all other functions
- lambda operators

## Further information
- [OData v4](https://docs.oasis-open.org/odata/odata/v4.01/odata-v4.01-part1-protocol.html)
- [OData Getting Started Tutorial](https://www.odata.org/getting-started/basic-tutorial)

