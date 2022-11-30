# Get periodized actor master data

**DEPRECATED** - see the [README](../README.md).

HTTP REST endpoint, which - given an actor number - returns the periods for the actor.

## Url

`GET /actors/<id>/periodized`

## Result Format

Data is returned as a JSON object with this format:

```json
{
  "id": <id>,
  "periods": [
    "roles: [
      <role>*
    ],
	"fromDate": <date time UTC>,
	"toDate": <date time UTC|null>
  ]
}
```

The periods are not overlapping and have no gaps between. The last period will always have an open-ended period (`toDate` is null).

## Explanation:

- `<id>`: An id uniquely identifying the actor
- `<role>`: "DDQ" | "MDR" | "DDM" | "DDK" | "DDZ" | "DDX"
- `<date time UTC>`: A date time ISO-8601 format string in UTC. Precision is minutes. Is always the moment of a danish date change.
    `fromDate` is inclusive. `toDate` is exclusive. The `toDate` of the last period is null.

## Example Result:

History for actor with id `3433d1d4-2fb8-440d-ba78-f3aa9a8b49e1`. Since 1st of January 2019 it has the role "DDM". From 1st of September 2022 it has both the "DDM" and "MDR" role.

```json
{
  "id": "3433d1d4-2fb8-440d-ba78-f3aa9a8b49e1",
  "periods": [
    {
	  "roles": [
	    "DDM"
	  ],
	  "fromDate": "2018-12-31T22:00Z",
	  "toDate": "2022-08-31T22:00Z"
	},
    {
	  "roles": [
	    "DDM",
		"MDR"
	  ],
	  "fromDate": "2022-08-31T22:00Z",
	  "toDate": null
	}
  ]
}
```
