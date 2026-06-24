namespace HealthPlatform.Infrastructure.Search.Indices;

internal static class SearchIndexMappings
{
    internal const string Doctor = """
        {
          "properties": {
            "doctorId": { "type": "keyword" },
            "name": {
              "type": "text",
              "fields": {
                "keyword": { "type": "keyword", "ignore_above": 256 }
              }
            },
            "specialty": { "type": "keyword" },
            "averageRating": { "type": "double" },
            "totalReviews": { "type": "integer" },
            "clinicLocation": { "type": "geo_point" },
            "virtualFee": { "type": "double" },
            "physicalFee": { "type": "double" },
            "minFee": { "type": "double" },
            "maxFee": { "type": "double" },
            "hasAvailability": { "type": "boolean" },
            "isSearchable": { "type": "boolean" },
            "availability": {
              "type": "nested",
              "properties": {
                "dayOfWeek": { "type": "integer" },
                "startTime": { "type": "keyword" },
                "endTime": { "type": "keyword" },
                "slotDurationMinutes": { "type": "integer" },
                "appointmentType": { "type": "keyword" },
                "isActive": { "type": "boolean" }
              }
            }
          }
        }
        """;

    internal const string Pharmacy = """
        {
          "properties": {
            "pharmacyId": { "type": "keyword" },
            "name": {
              "type": "text",
              "fields": {
                "keyword": { "type": "keyword", "ignore_above": 256 }
              }
            },
            "address": { "type": "text" },
            "location": { "type": "geo_point" },
            "hasStock": { "type": "boolean" },
            "isSearchable": { "type": "boolean" },
            "stockSummary": {
              "type": "nested",
              "properties": {
                "medicationName": { "type": "keyword" },
                "medicationSku": { "type": "keyword" },
                "quantityOnHand": { "type": "integer" }
              }
            }
          }
        }
        """;

    internal const string LabPartner = """
        {
          "properties": {
            "labPartnerId": { "type": "keyword" },
            "name": {
              "type": "text",
              "fields": {
                "keyword": { "type": "keyword", "ignore_above": 256 }
              }
            },
            "address": { "type": "text" },
            "location": { "type": "geo_point" },
            "testTypes": { "type": "keyword" },
            "isSearchable": { "type": "boolean" },
            "pricing": {
              "type": "nested",
              "properties": {
                "testType": { "type": "keyword" },
                "price": { "type": "double" },
                "currency": { "type": "keyword" }
              }
            }
          }
        }
        """;
}
