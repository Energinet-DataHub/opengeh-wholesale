{
  "lenses": {
    "0": {
      "order": 0,
      "parts": {
        "0": {
          "position": {
            "x": 0,
            "y": 0,
            "colSpan": 6,
            "rowSpan": 4
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "isOptional": true
              },
              {
                "name": "sharedTimeRange",
                "isOptional": true
              }
            ],
            "type": "Extension/HubsExtension/PartType/MonitorChartPart",
            "settings": {
              "content": {
                "options": {
                  "chart": {
                    "metrics": [
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${shared_resources_resource_group_name}/providers/Microsoft.Web/serverFarms/${shared_plan_name}"
                        },
                        "name": "CpuPercentage",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/serverfarms",
                        "metricVisualization": {
                          "displayName": "CPU Percentage",
                          "resourceDisplayName": "${shared_plan_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${shared_resources_resource_group_name}/providers/Microsoft.Web/serverFarms/${shared_plan_name}"
                        },
                        "name": "MemoryPercentage",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/serverfarms",
                        "metricVisualization": {
                          "displayName": "Memory Percentage",
                          "resourceDisplayName": "${shared_plan_name}"
                        }
                      }
                    ],
                    "title": "Avg CPU Percentage and Avg Memory Percentage for ${shared_plan_name}",
                    "titleKind": 1,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideSubtitle": false
                      },
                      "axisVisualization": {
                        "x": {
                          "isVisible": true,
                          "axisType": 2
                        },
                        "y": {
                          "isVisible": true,
                          "axisType": 1
                        }
                      },
                      "disablePinning": true
                    }
                  }
                }
              }
            }
          }
        },
        "1": {
          "position": {
            "x": 6,
            "y": 0,
            "colSpan": 6,
            "rowSpan": 4
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "isOptional": true
              },
              {
                "name": "sharedTimeRange",
                "isOptional": true
              }
            ],
            "type": "Extension/HubsExtension/PartType/MonitorChartPart",
            "settings": {
              "content": {
                "options": {
                  "chart": {
                    "metrics": [
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${charges_resource_group_name}/providers/Microsoft.Web/sites/${charges_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${charges_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${aggregations_resource_group_name}/providers/Microsoft.Web/sites/${aggregations_coordinator_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${aggregations_coordinator_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${aggregations_resource_group_name}/providers/Microsoft.Web/sites/${aggregations_integration_event_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${aggregations_integration_event_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${market_participant_resource_group_name}/providers/Microsoft.Web/sites/${market_participant_organization_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${market_participant_organization_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${market_roles_resource_group_name}/providers/Microsoft.Web/sites/${market_roles_api_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${market_roles_api_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${market_roles_resource_group_name}/providers/Microsoft.Web/sites/${market_roles_ingestion_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${market_roles_ingestion_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${market_roles_resource_group_name}/providers/Microsoft.Web/sites/${market_roles_internalcommanddispatcher_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${market_roles_internalcommanddispatcher_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${market_roles_resource_group_name}/providers/Microsoft.Web/sites/${market_roles_localmessagehub_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${market_roles_localmessagehub_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${market_roles_resource_group_name}/providers/Microsoft.Web/sites/${market_roles_outbox_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${market_roles_outbox_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${market_roles_resource_group_name}/providers/Microsoft.Web/sites/${market_roles_processing_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${market_roles_processing_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${message_archive_resource_group_name}/providers/Microsoft.Web/sites/${message_archive_entrypoint_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${message_archive_entrypoint_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${metering_point_resource_group_name}/providers/Microsoft.Web/sites/${metering_point_ingestion_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${metering_point_ingestion_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${metering_point_resource_group_name}/providers/Microsoft.Web/sites/${metering_point_localmessagehub_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${metering_point_localmessagehub_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${metering_point_resource_group_name}/providers/Microsoft.Web/sites/${metering_point_outbox_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${metering_point_outbox_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${metering_point_resource_group_name}/providers/Microsoft.Web/sites/${metering_point_processing_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${metering_point_processing_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${post_office_resource_group_name}/providers/Microsoft.Web/sites/${post_office_marketoperator_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${post_office_marketoperator_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${post_office_resource_group_name}/providers/Microsoft.Web/sites/${post_office_operations_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${post_office_operations_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${post_office_resource_group_name}/providers/Microsoft.Web/sites/${post_office_subdomain_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${post_office_subdomain_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${time_series_resource_group_name}/providers/Microsoft.Web/sites/${time_series_bundle_ingestor_function_name}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units",
                          "resourceDisplayName": "${time_series_bundle_ingestor_function_name}"
                        }
                      }
                    ],
                    "title": "Avg Function Execution Units per Azure Function",
                    "titleKind": 1,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideSubtitle": false
                      },
                      "axisVisualization": {
                        "x": {
                          "isVisible": true,
                          "axisType": 2
                        },
                        "y": {
                          "isVisible": true,
                          "axisType": 1
                        }
                      },
                      "disablePinning": true
                    }
                  }
                }
              }
            }
          }
        },
        "2": {
          "position": {
            "x": 12,
            "y": 0,
            "colSpan": 6,
            "rowSpan": 4
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "isOptional": true
              },
              {
                "name": "sharedTimeRange",
                "isOptional": true
              }
            ],
            "type": "Extension/HubsExtension/PartType/MonitorChartPart",
            "settings": {
              "content": {
                "options": {
                  "chart": {
                    "metrics": [
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${charges_resource_group_name}/providers/Microsoft.Web/sites/${charges_webapi_name}"
                        },
                        "name": "CpuTime",
                        "aggregationType": 1,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "CPU Time",
                          "resourceDisplayName": "${charges_webapi_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${frontend_resource_group_name}/providers/Microsoft.Web/sites/${frontend_bff_webapi_name}"
                        },
                        "name": "CpuTime",
                        "aggregationType": 1,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "CPU Time",
                          "resourceDisplayName": "${frontend_bff_webapi_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${market_participant_resource_group_name}/providers/Microsoft.Web/sites/${market_participant_webapi_name}"
                        },
                        "name": "CpuTime",
                        "aggregationType": 1,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "CPU Time",
                          "resourceDisplayName": "${market_participant_webapi_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${message_archive_resource_group_name}/providers/Microsoft.Web/sites/${message_archive_webapi_name}"
                        },
                        "name": "CpuTime",
                        "aggregationType": 1,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "CPU Time",
                          "resourceDisplayName": "${message_archive_webapi_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${metering_point_resource_group_name}/providers/Microsoft.Web/sites/${metering_point_webapi_name}"
                        },
                        "name": "CpuTime",
                        "aggregationType": 1,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "CPU Time",
                          "resourceDisplayName": "${metering_point_webapi_name}"
                        }
                      }
                    ],
                    "title": "Sum CPU Time per Web API",
                    "titleKind": 1,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideSubtitle": false
                      },
                      "axisVisualization": {
                        "x": {
                          "isVisible": true,
                          "axisType": 2
                        },
                        "y": {
                          "isVisible": true,
                          "axisType": 1
                        }
                      },
                      "disablePinning": true
                    }
                  }
                }
              }
            }
          }
        },
        "3": {
          "position": {
            "x": 6,
            "y": 4,
            "colSpan": 6,
            "rowSpan": 4
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "isOptional": true
              },
              {
                "name": "sharedTimeRange",
                "isOptional": true
              }
            ],
            "type": "Extension/HubsExtension/PartType/MonitorChartPart",
            "settings": {
              "content": {
                "options": {
                  "chart": {
                    "metrics": [
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${aggregations_resource_group_name}/providers/Microsoft.Web/sites/${aggregations_coordinator_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${aggregations_coordinator_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${aggregations_resource_group_name}/providers/Microsoft.Web/sites/${aggregations_integration_event_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${aggregations_integration_event_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${charges_resource_group_name}/providers/Microsoft.Web/sites/${charges_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${charges_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${market_participant_resource_group_name}/providers/Microsoft.Web/sites/${market_participant_organization_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${market_participant_organization_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${market_roles_resource_group_name}/providers/Microsoft.Web/sites/${market_roles_api_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${market_roles_api_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${market_roles_resource_group_name}/providers/Microsoft.Web/sites/${market_roles_ingestion_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${market_roles_ingestion_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${market_roles_resource_group_name}/providers/Microsoft.Web/sites/${market_roles_internalcommanddispatcher_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${market_roles_internalcommanddispatcher_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${market_roles_resource_group_name}/providers/Microsoft.Web/sites/${market_roles_localmessagehub_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${market_roles_localmessagehub_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${market_roles_resource_group_name}/providers/Microsoft.Web/sites/${market_roles_outbox_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${market_roles_outbox_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${market_roles_resource_group_name}/providers/Microsoft.Web/sites/${market_roles_processing_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${market_roles_processing_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${message_archive_resource_group_name}/providers/Microsoft.Web/sites/${message_archive_entrypoint_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${message_archive_entrypoint_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${metering_point_resource_group_name}/providers/Microsoft.Web/sites/${metering_point_ingestion_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${metering_point_ingestion_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${metering_point_resource_group_name}/providers/Microsoft.Web/sites/${metering_point_localmessagehub_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${metering_point_localmessagehub_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${metering_point_resource_group_name}/providers/Microsoft.Web/sites/${metering_point_outbox_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${metering_point_outbox_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${metering_point_resource_group_name}/providers/Microsoft.Web/sites/${metering_point_processing_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${metering_point_processing_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${post_office_resource_group_name}/providers/Microsoft.Web/sites/${post_office_marketoperator_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${post_office_marketoperator_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${post_office_resource_group_name}/providers/Microsoft.Web/sites/${post_office_operations_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${post_office_operations_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${post_office_resource_group_name}/providers/Microsoft.Web/sites/${post_office_subdomain_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${post_office_subdomain_function_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${time_series_resource_group_name}/providers/Microsoft.Web/sites/${time_series_bundle_ingestor_function_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${time_series_bundle_ingestor_function_name}"
                        }
                      }
                    ],
                    "title": "Avg Average memory working set per Azure Function",
                    "titleKind": 1,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideSubtitle": false
                      },
                      "axisVisualization": {
                        "x": {
                          "isVisible": true,
                          "axisType": 2
                        },
                        "y": {
                          "isVisible": true,
                          "axisType": 1
                        }
                      },
                      "disablePinning": true
                    }
                  }
                }
              }
            }
          }
        },
        "4": {
          "position": {
            "x": 12,
            "y": 4,
            "colSpan": 6,
            "rowSpan": 4
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "isOptional": true
              },
              {
                "name": "sharedTimeRange",
                "isOptional": true
              }
            ],
            "type": "Extension/HubsExtension/PartType/MonitorChartPart",
            "settings": {
              "content": {
                "options": {
                  "chart": {
                    "metrics": [
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${charges_resource_group_name}/providers/Microsoft.Web/sites/${charges_webapi_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${charges_webapi_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${frontend_resource_group_name}/providers/Microsoft.Web/sites/${frontend_bff_webapi_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${frontend_bff_webapi_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${market_participant_resource_group_name}/providers/Microsoft.Web/sites/${market_participant_webapi_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${market_participant_webapi_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${message_archive_resource_group_name}/providers/Microsoft.Web/sites/${message_archive_webapi_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${message_archive_webapi_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${subscription_id}/resourceGroups/${metering_point_resource_group_name}/providers/Microsoft.Web/sites/${metering_point_webapi_name}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set",
                          "resourceDisplayName": "${metering_point_webapi_name}"
                        }
                      }
                    ],
                    "title": "Avg Average memory working set per Web API",
                    "titleKind": 1,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideSubtitle": false
                      },
                      "axisVisualization": {
                        "x": {
                          "isVisible": true,
                          "axisType": 2
                        },
                        "y": {
                          "isVisible": true,
                          "axisType": 1
                        }
                      },
                      "disablePinning": true
                    }
                  }
                }
              }
            }
          }
        }
      }
    }
  },
  "metadata": {
    "model": {
      "timeRange": {
        "value": {
          "relative": {
            "duration": 24,
            "timeUnit": 1
          }
        },
        "type": "MsPortalFx.Composition.Configuration.ValueTypes.TimeRange"
      },
      "filterLocale": {
        "value": "en-us"
      },
      "filters": {
        "value": {
          "MsPortalFx_TimeRange": {
            "model": {
              "format": "utc",
              "granularity": "auto",
              "relative": "24h"
            },
            "displayCache": {
              "name": "UTC Time",
              "value": "Past 24 hours"
            },
            "filteredPartIds": [
              "StartboardPart-MonitorChartPart-2696c7ee-b807-4f23-8c85-d18437bdfa69",
              "StartboardPart-MonitorChartPart-2696c7ee-b807-4f23-8c85-d18437bdfcda",
              "StartboardPart-MonitorChartPart-2fb11779-26e5-4488-9cf1-fcb9c8214108",
              "StartboardPart-MonitorChartPart-20f84b6b-1aa6-4d05-b7d8-9b9f80aca2d5",
              "StartboardPart-MonitorChartPart-20f84b6b-1aa6-4d05-b7d8-9b9f80acaac2"
            ]
          }
        }
      }
    }
  }
}