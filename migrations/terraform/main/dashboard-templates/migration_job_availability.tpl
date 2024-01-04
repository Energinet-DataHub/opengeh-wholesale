{
  "lenses": {
    "0": {
      "order": 0,
      "parts": {
        "0": {
          "position": {
            "x": 0,
            "y": 0,
            "colSpan": 15,
            "rowSpan": 1
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "content": "<div style=\"background-color: #1976d2; color: #ffffff; padding: 18px;\">\r\n   **Wholesale**\r\n</div>",
                "title": "",
                "subtitle": "",
                "markdownSource": 1,
                "markdownUri": ""
              }
            }
          }
        },
        "1": {
          "position": {
            "x": 0,
            "y": 1,
            "colSpan": 4,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "sharedTimeRange",
                "isOptional": true
              },
              {
                "name": "options",
                "value": {
                  "chart": {
                    "metrics": [
                      {
                        "resourceMetadata": {
                          "id": "${shared_storage_id}"
                        },
                        "name": "Availability",
                        "aggregationType": 4,
                        "namespace": "microsoft.storage/storageaccounts/blobservices",
                        "metricVisualization": {
                          "displayName": "Availability"
                        }
                      }
                    ],
                    "title": "Avg Availability for ${shared_storage_name}",
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
                      }
                    },
                    "timespan": {
                      "relative": {
                        "duration": 86400000
                      },
                      "showUTCTime": false,
                      "grain": 1
                    }
                  }
                },
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
                          "id": "${shared_storage_id}"
                        },
                        "name": "Availability",
                        "aggregationType": 4,
                        "namespace": "microsoft.storage/storageaccounts/blobservices",
                        "metricVisualization": {
                          "displayName": "Availability"
                        }
                      }
                    ],
                    "title": "Avg Availability for ${shared_storage_name}",
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
            "x": 4,
            "y": 1,
            "colSpan": 4,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "resourceTypeMode",
                "isOptional": true
              },
              {
                "name": "ComponentId",
                "isOptional": true
              },
              {
                "name": "Scope",
                "value": {
                  "resourceIds": [
                    "${log_analytics_workspace_id}"
                  ]
                },
                "isOptional": true
              },
              {
                "name": "PartId",
                "value": "1074811e-1213-4059-ac14-c95910a11729",
                "isOptional": true
              },
              {
                "name": "Version",
                "value": "2.0",
                "isOptional": true
              },
              {
                "name": "TimeRange",
                "value": "P1D",
                "isOptional": true
              },
              {
                "name": "DashboardId",
                "isOptional": true
              },
              {
                "name": "DraftRequestParameters",
                "value": {
                  "scope": "hierarchy"
                },
                "isOptional": true
              },
              {
                "name": "Query",
                "value": "let taskKey = \"monitor_wholesale_metering_points\";\nlet failed =\n    DatabricksJobs\n    | where RequestParams has taskKey\n    | where ActionName in (\"runFailed\")\n    | count;\nlet succeeded = \n    DatabricksJobs\n    | where RequestParams has taskKey\n    | where ActionName in (\"runSucceeded\")\n    | count;\nunion\n    (DatabricksJobs\n    | take 1\n    | project\n        Metric = \"availability %\",\n        Value = round(todecimal(toscalar(succeeded)) / todecimal(toscalar(succeeded) + toscalar(failed)) * 100, 2)\n    ),\n    (DatabricksJobs\n    | take 1\n    | project Metric = \"succeeded\", Value = toreal(toscalar(succeeded))\n    ),\n    (DatabricksJobs\n    | take 1\n    | project Metric = \"failed\", Value = toreal(toscalar(failed))\n    ),\n    (DatabricksJobs\n    | take 1\n    | project Metric = \"total\", Value = toreal(toscalar(failed)) + toreal(toscalar(succeeded))\n    )\n| order by Metric asc\n\n",
                "isOptional": true
              },
              {
                "name": "ControlType",
                "value": "AnalyticsGrid",
                "isOptional": true
              },
              {
                "name": "SpecificChart",
                "isOptional": true
              },
              {
                "name": "PartTitle",
                "value": "Analytics",
                "isOptional": true
              },
              {
                "name": "PartSubTitle",
                "value": "${shared_resources_resource_group_name}",
                "isOptional": true
              },
              {
                "name": "Dimensions",
                "isOptional": true
              },
              {
                "name": "LegendOptions",
                "isOptional": true
              },
              {
                "name": "IsQueryContainTimeRange",
                "value": false,
                "isOptional": true
              }
            ],
            "type": "Extension/Microsoft_OperationsManagementSuite_Workspace/PartType/LogsDashboardPart",
            "settings": {
              "content": {
                "PartTitle": "metering_point_periods",
                "PartSubTitle": "Wholesale"
              }
            }
          }
        },
        "3": {
          "position": {
            "x": 8,
            "y": 1,
            "colSpan": 7,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "resourceTypeMode",
                "isOptional": true
              },
              {
                "name": "ComponentId",
                "isOptional": true
              },
              {
                "name": "Scope",
                "value": {
                  "resourceIds": [
                    "${log_analytics_workspace_id}"
                  ]
                },
                "isOptional": true
              },
              {
                "name": "PartId",
                "value": "69993707-86c3-48cc-a134-66a5a914b57f",
                "isOptional": true
              },
              {
                "name": "Version",
                "value": "2.0",
                "isOptional": true
              },
              {
                "name": "TimeRange",
                "value": "P7D",
                "isOptional": true
              },
              {
                "name": "DashboardId",
                "isOptional": true
              },
              {
                "name": "DraftRequestParameters",
                "value": {
                  "scope": "hierarchy"
                },
                "isOptional": true
              },
              {
                "name": "Query",
                "value": "let taskKey = \"monitor_eloverblik_time_series\";\nDatabricksJobs\n| where RequestParams has taskKey\n| where ActionName in (\"runFailed\")\n| project runId = todecimal(parse_json(RequestParams).multitaskParentRunId), TimeGenerated\n| order by TimeGenerated asc\n",
                "isOptional": true
              },
              {
                "name": "ControlType",
                "value": "FrameControlChart",
                "isOptional": true
              },
              {
                "name": "SpecificChart",
                "value": "PercentageColumn",
                "isOptional": true
              },
              {
                "name": "PartTitle",
                "value": "Analytics",
                "isOptional": true
              },
              {
                "name": "PartSubTitle",
                "value": "${shared_resources_resource_group_name}",
                "isOptional": true
              },
              {
                "name": "Dimensions",
                "value": {
                  "xAxis": {
                    "name": "TimeGenerated",
                    "type": "datetime"
                  },
                  "yAxis": [
                    {
                      "name": "runId",
                      "type": "decimal"
                    }
                  ],
                  "splitBy": [],
                  "aggregation": "Sum"
                },
                "isOptional": true
              },
              {
                "name": "LegendOptions",
                "value": {
                  "isEnabled": false,
                  "position": "Bottom"
                },
                "isOptional": true
              },
              {
                "name": "IsQueryContainTimeRange",
                "value": false,
                "isOptional": true
              }
            ],
            "type": "Extension/Microsoft_OperationsManagementSuite_Workspace/PartType/LogsDashboardPart",
            "settings": {
              "content": {
                "Query": "let job = \"monitor_wholesale_metering_points\";\nlet timeRangeEnd = todatetime(now() - 5m);\nlet timeRangeStart = startofday(timeRangeEnd - 30d);\nlet timeRange = range runTime from timeRangeStart to timeRangeEnd step 5m;\nlet data = DatabricksJobs\n    | where RequestParams has job\n    | project task = job, ActionName, runTime = todatetime(bin(TimeGenerated, 5m))\n    | where ActionName in (\"runStart\")\n    | summarize runs = count() by runTime, task;\ntimeRange \n    | extend task = job\n    | join kind=leftouter data on runTime and task\n    | project runTime, task, runs = iif(runs>0, 1, 0)\n    | order by runTime asc, task asc\n\n",
                "ControlType": "FrameControlChart",
                "SpecificChart": "Line",
                "PartTitle": "Job availability - monitor_wholesale_metering_points",
                "PartSubTitle": "Job executed within a 5 min. timespan for 30 days",
                "Dimensions": {
                  "xAxis": {
                    "name": "runTime",
                    "type": "datetime"
                  },
                  "yAxis": [
                    {
                      "name": "runs",
                      "type": "long"
                    }
                  ],
                  "splitBy": [
                    {
                      "name": "task",
                      "type": "string"
                    }
                  ],
                  "aggregation": "Sum"
                },
                "LegendOptions": {
                  "isEnabled": true,
                  "position": "Bottom"
                }
              }
            },
            "filters": {
              "MsPortalFx_TimeRange": {
                "model": {
                  "format": "local",
                  "granularity": "auto",
                  "relative": "30d"
                }
              }
            }
          }
        },
        "4": {
          "position": {
            "x": 4,
            "y": 4,
            "colSpan": 4,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "resourceTypeMode",
                "isOptional": true
              },
              {
                "name": "ComponentId",
                "isOptional": true
              },
              {
                "name": "Scope",
                "value": {
                  "resourceIds": [
                    "${log_analytics_workspace_id}"
                  ]
                },
                "isOptional": true
              },
              {
                "name": "PartId",
                "value": "15b066e6-063f-4c79-9c4d-20d95e09849a",
                "isOptional": true
              },
              {
                "name": "Version",
                "value": "2.0",
                "isOptional": true
              },
              {
                "name": "TimeRange",
                "value": "P1D",
                "isOptional": true
              },
              {
                "name": "DashboardId",
                "isOptional": true
              },
              {
                "name": "DraftRequestParameters",
                "value": {
                  "scope": "hierarchy"
                },
                "isOptional": true
              },
              {
                "name": "Query",
                "value": "let taskKey = \"monitor_wholesale_time_series\";\nlet failed =\n    DatabricksJobs\n    | where RequestParams has taskKey\n    | where ActionName in (\"runFailed\")\n    | count;\nlet succeeded = \n    DatabricksJobs\n    | where RequestParams has taskKey\n    | where ActionName in (\"runSucceeded\")\n    | count;\nunion\n    (DatabricksJobs\n    | take 1\n    | project\n        Metric = \"availability %\",\n        Value = round(todecimal(toscalar(succeeded)) / todecimal(toscalar(succeeded) + toscalar(failed)) * 100, 2)\n    ),\n    (DatabricksJobs\n    | take 1\n    | project Metric = \"succeeded\", Value = toreal(toscalar(succeeded))\n    ),\n    (DatabricksJobs\n    | take 1\n    | project Metric = \"failed\", Value = toreal(toscalar(failed))\n    ),\n    (DatabricksJobs\n    | take 1\n    | project Metric = \"total\", Value = toreal(toscalar(failed)) + toreal(toscalar(succeeded))\n    )\n| order by Metric asc\n",
                "isOptional": true
              },
              {
                "name": "ControlType",
                "value": "AnalyticsGrid",
                "isOptional": true
              },
              {
                "name": "SpecificChart",
                "isOptional": true
              },
              {
                "name": "PartTitle",
                "value": "Analytics",
                "isOptional": true
              },
              {
                "name": "PartSubTitle",
                "value": "${shared_resources_resource_group_name}",
                "isOptional": true
              },
              {
                "name": "Dimensions",
                "isOptional": true
              },
              {
                "name": "LegendOptions",
                "isOptional": true
              },
              {
                "name": "IsQueryContainTimeRange",
                "value": false,
                "isOptional": true
              }
            ],
            "type": "Extension/Microsoft_OperationsManagementSuite_Workspace/PartType/LogsDashboardPart",
            "settings": {
              "content": {
                "PartTitle": "time_series_points",
                "PartSubTitle": "Wholesale"
              }
            }
          }
        },
        "5": {
          "position": {
            "x": 8,
            "y": 4,
            "colSpan": 7,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "resourceTypeMode",
                "isOptional": true
              },
              {
                "name": "ComponentId",
                "isOptional": true
              },
              {
                "name": "Scope",
                "value": {
                  "resourceIds": [
                    "${log_analytics_workspace_id}"
                  ]
                },
                "isOptional": true
              },
              {
                "name": "PartId",
                "value": "69993707-86c3-48cc-a134-66a5a914b57f",
                "isOptional": true
              },
              {
                "name": "Version",
                "value": "2.0",
                "isOptional": true
              },
              {
                "name": "TimeRange",
                "value": "P7D",
                "isOptional": true
              },
              {
                "name": "DashboardId",
                "isOptional": true
              },
              {
                "name": "DraftRequestParameters",
                "value": {
                  "scope": "hierarchy"
                },
                "isOptional": true
              },
              {
                "name": "Query",
                "value": "let taskKey = \"monitor_eloverblik_time_series\";\nDatabricksJobs\n| where RequestParams has taskKey\n| where ActionName in (\"runFailed\")\n| project runId = todecimal(parse_json(RequestParams).multitaskParentRunId), TimeGenerated\n| order by TimeGenerated asc\n",
                "isOptional": true
              },
              {
                "name": "ControlType",
                "value": "FrameControlChart",
                "isOptional": true
              },
              {
                "name": "SpecificChart",
                "value": "PercentageColumn",
                "isOptional": true
              },
              {
                "name": "PartTitle",
                "value": "Analytics",
                "isOptional": true
              },
              {
                "name": "PartSubTitle",
                "value": "${shared_resources_resource_group_name}",
                "isOptional": true
              },
              {
                "name": "Dimensions",
                "value": {
                  "xAxis": {
                    "name": "TimeGenerated",
                    "type": "datetime"
                  },
                  "yAxis": [
                    {
                      "name": "runId",
                      "type": "decimal"
                    }
                  ],
                  "splitBy": [],
                  "aggregation": "Sum"
                },
                "isOptional": true
              },
              {
                "name": "LegendOptions",
                "value": {
                  "isEnabled": false,
                  "position": "Bottom"
                },
                "isOptional": true
              },
              {
                "name": "IsQueryContainTimeRange",
                "value": false,
                "isOptional": true
              }
            ],
            "type": "Extension/Microsoft_OperationsManagementSuite_Workspace/PartType/LogsDashboardPart",
            "settings": {
              "content": {
                "Query": "let job = \"monitor_wholesale_time_series\";\nlet timeRangeEnd = todatetime(now() - 5m);\nlet timeRangeStart = startofday(timeRangeEnd - 30d);\nlet timeRange = range runTime from timeRangeStart to timeRangeEnd step 5m;\nlet data = DatabricksJobs\n    | where RequestParams has job\n    | project task = job, ActionName, runTime = todatetime(bin(TimeGenerated, 5m))\n    | where ActionName in (\"runStart\")\n    | summarize runs = count() by runTime, task;\ntimeRange \n    | extend task = job\n    | join kind=leftouter data on runTime and task\n    | project runTime, task, runs = iif(runs>0, 1, 0)\n    | order by runTime asc, task asc\n\n",
                "ControlType": "FrameControlChart",
                "SpecificChart": "Line",
                "PartTitle": "Job availability - monitor_wholesale_time_series",
                "PartSubTitle": "Job executed within a 5 min. timespan for 30 days",
                "Dimensions": {
                  "xAxis": {
                    "name": "runTime",
                    "type": "datetime"
                  },
                  "yAxis": [
                    {
                      "name": "runs",
                      "type": "long"
                    }
                  ],
                  "splitBy": [
                    {
                      "name": "task",
                      "type": "string"
                    }
                  ],
                  "aggregation": "Sum"
                },
                "LegendOptions": {
                  "isEnabled": true,
                  "position": "Bottom"
                }
              }
            },
            "filters": {
              "MsPortalFx_TimeRange": {
                "model": {
                  "format": "local",
                  "granularity": "auto",
                  "relative": "30d"
                }
              }
            }
          }
        },
        "6": {
          "position": {
            "x": 0,
            "y": 7,
            "colSpan": 15,
            "rowSpan": 1
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "content": "<div style=\"background-color: #1976d2; color: #ffffff; padding: 18px;\">\n   **Eloverblik**\n</div>\n",
                "title": "",
                "subtitle": "",
                "markdownSource": 1,
                "markdownUri": ""
              }
            }
          }
        },
        "7": {
          "position": {
            "x": 0,
            "y": 8,
            "colSpan": 4,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "sharedTimeRange",
                "isOptional": true
              },
              {
                "name": "options",
                "value": {
                  "chart": {
                    "metrics": [
                      {
                        "resourceMetadata": {
                          "id": "${migration_storage_id}"
                        },
                        "name": "Availability",
                        "aggregationType": 4,
                        "namespace": "microsoft.storage/storageaccounts/blobservices",
                        "metricVisualization": {
                          "displayName": "Availability"
                        }
                      }
                    ],
                    "title": "Avg Availability for ${migration_storage_name}",
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
                      }
                    },
                    "timespan": {
                      "relative": {
                        "duration": 86400000
                      },
                      "showUTCTime": false,
                      "grain": 1
                    }
                  }
                },
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
                          "id": "${migration_storage_id}"
                        },
                        "name": "Availability",
                        "aggregationType": 4,
                        "namespace": "microsoft.storage/storageaccounts/blobservices",
                        "metricVisualization": {
                          "displayName": "Availability"
                        }
                      }
                    ],
                    "title": "Avg Availability for ${migration_storage_name}",
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
        "8": {
          "position": {
            "x": 4,
            "y": 8,
            "colSpan": 4,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "resourceTypeMode",
                "isOptional": true
              },
              {
                "name": "ComponentId",
                "isOptional": true
              },
              {
                "name": "Scope",
                "value": {
                  "resourceIds": [
                    "${log_analytics_workspace_id}"
                  ]
                },
                "isOptional": true
              },
              {
                "name": "PartId",
                "value": "24669452-0505-472c-866c-b404d8b4dc1a",
                "isOptional": true
              },
              {
                "name": "Version",
                "value": "2.0",
                "isOptional": true
              },
              {
                "name": "TimeRange",
                "value": "P1D",
                "isOptional": true
              },
              {
                "name": "DashboardId",
                "isOptional": true
              },
              {
                "name": "DraftRequestParameters",
                "value": {
                  "scope": "hierarchy"
                },
                "isOptional": true
              },
              {
                "name": "Query",
                "value": "let taskKey = \"monitor_eloverblik_time_series\";\nlet failed =\n    DatabricksJobs\n    | where RequestParams has taskKey\n    | where ActionName in (\"runFailed\")\n    | count;\nlet succeeded = \n    DatabricksJobs\n    | where RequestParams has taskKey\n    | where ActionName in (\"runSucceeded\")\n    | count;\nunion\n    (DatabricksJobs\n    | take 1\n    | project\n        Metric = \"availability %\",\n        Value = round(todecimal(toscalar(succeeded)) / todecimal(toscalar(succeeded) + toscalar(failed)) * 100, 2)\n    ),\n    (DatabricksJobs\n    | take 1\n    | project Metric = \"succeeded\", Value = toreal(toscalar(succeeded))\n    ),\n    (DatabricksJobs\n    | take 1\n    | project Metric = \"failed\", Value = toreal(toscalar(failed))\n    ),\n    (DatabricksJobs\n    | take 1\n    | project Metric = \"total\", Value = toreal(toscalar(failed)) + toreal(toscalar(succeeded))\n    )\n| order by Metric asc\n\n",
                "isOptional": true
              },
              {
                "name": "ControlType",
                "value": "AnalyticsGrid",
                "isOptional": true
              },
              {
                "name": "SpecificChart",
                "isOptional": true
              },
              {
                "name": "PartTitle",
                "value": "Analytics",
                "isOptional": true
              },
              {
                "name": "PartSubTitle",
                "value": "${shared_resources_resource_group_name}",
                "isOptional": true
              },
              {
                "name": "Dimensions",
                "isOptional": true
              },
              {
                "name": "LegendOptions",
                "isOptional": true
              },
              {
                "name": "IsQueryContainTimeRange",
                "value": false,
                "isOptional": true
              }
            ],
            "type": "Extension/Microsoft_OperationsManagementSuite_Workspace/PartType/LogsDashboardPart",
            "settings": {
              "content": {
                "PartTitle": "eloverblik_time_series_points",
                "PartSubTitle": "Eloverblik"
              }
            }
          }
        },
        "9": {
          "position": {
            "x": 8,
            "y": 8,
            "colSpan": 7,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "resourceTypeMode",
                "isOptional": true
              },
              {
                "name": "ComponentId",
                "isOptional": true
              },
              {
                "name": "Scope",
                "value": {
                  "resourceIds": [
                    "${log_analytics_workspace_id}"
                  ]
                },
                "isOptional": true
              },
              {
                "name": "PartId",
                "value": "69993707-86c3-48cc-a134-66a5a914b57f",
                "isOptional": true
              },
              {
                "name": "Version",
                "value": "2.0",
                "isOptional": true
              },
              {
                "name": "TimeRange",
                "value": "P7D",
                "isOptional": true
              },
              {
                "name": "DashboardId",
                "isOptional": true
              },
              {
                "name": "DraftRequestParameters",
                "value": {
                  "scope": "hierarchy"
                },
                "isOptional": true
              },
              {
                "name": "Query",
                "value": "let taskKey = \"monitor_eloverblik_time_series\";\nDatabricksJobs\n| where RequestParams has taskKey\n| where ActionName in (\"runFailed\")\n| project runId = todecimal(parse_json(RequestParams).multitaskParentRunId), TimeGenerated\n| order by TimeGenerated asc\n",
                "isOptional": true
              },
              {
                "name": "ControlType",
                "value": "FrameControlChart",
                "isOptional": true
              },
              {
                "name": "SpecificChart",
                "value": "PercentageColumn",
                "isOptional": true
              },
              {
                "name": "PartTitle",
                "value": "Analytics",
                "isOptional": true
              },
              {
                "name": "PartSubTitle",
                "value": "${shared_resources_resource_group_name}",
                "isOptional": true
              },
              {
                "name": "Dimensions",
                "value": {
                  "xAxis": {
                    "name": "TimeGenerated",
                    "type": "datetime"
                  },
                  "yAxis": [
                    {
                      "name": "runId",
                      "type": "decimal"
                    }
                  ],
                  "splitBy": [],
                  "aggregation": "Sum"
                },
                "isOptional": true
              },
              {
                "name": "LegendOptions",
                "value": {
                  "isEnabled": false,
                  "position": "Bottom"
                },
                "isOptional": true
              },
              {
                "name": "IsQueryContainTimeRange",
                "value": false,
                "isOptional": true
              }
            ],
            "type": "Extension/Microsoft_OperationsManagementSuite_Workspace/PartType/LogsDashboardPart",
            "settings": {
              "content": {
                "Query": "let job = \"monitor_eloverblik_time_series\";\nlet timeRangeEnd = todatetime(now() - 5m);\nlet timeRangeStart = startofday(timeRangeEnd - 30d);\nlet timeRange = range runTime from timeRangeStart to timeRangeEnd step 5m;\nlet data = DatabricksJobs\n    | where RequestParams has job\n    | project task = job, ActionName, runTime = todatetime(bin(TimeGenerated, 5m))\n    | where ActionName in (\"runStart\")\n    | summarize runs = count() by runTime, task;\ntimeRange \n    | extend task = job\n    | join kind=leftouter data on runTime and task\n    | project runTime, task, runs = iif(runs>0, 1, 0)\n    | order by runTime asc, task asc\n\n",
                "ControlType": "FrameControlChart",
                "SpecificChart": "Line",
                "PartTitle": "Job availability - monitor_eloverblik_time_series",
                "PartSubTitle": "Job executed within a 5 min. timespan for 30 days",
                "Dimensions": {
                  "xAxis": {
                    "name": "runTime",
                    "type": "datetime"
                  },
                  "yAxis": [
                    {
                      "name": "runs",
                      "type": "long"
                    }
                  ],
                  "splitBy": [
                    {
                      "name": "task",
                      "type": "string"
                    }
                  ],
                  "aggregation": "Sum"
                },
                "LegendOptions": {
                  "isEnabled": true,
                  "position": "Bottom"
                }
              }
            },
            "filters": {
              "MsPortalFx_TimeRange": {
                "model": {
                  "format": "local",
                  "granularity": "auto",
                  "relative": "30d"
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
              "format": "local",
              "granularity": "auto",
              "relative": "7d"
            },
            "displayCache": {
              "name": "Local Time",
              "value": "Past 7 days"
            },
            "filteredPartIds": [
              "StartboardPart-MonitorChartPart-e8684ceb-eecf-4f48-9685-7294c16775b0",
              "StartboardPart-LogsDashboardPart-e8684ceb-eecf-4f48-9685-7294c16775b2",
              "StartboardPart-LogsDashboardPart-e8684ceb-eecf-4f48-9685-7294c16775b4",
              "StartboardPart-LogsDashboardPart-e8684ceb-eecf-4f48-9685-7294c16775b6",
              "StartboardPart-LogsDashboardPart-e8684ceb-eecf-4f48-9685-7294c16775b8",
              "StartboardPart-MonitorChartPart-e8684ceb-eecf-4f48-9685-7294c16775bc",
              "StartboardPart-LogsDashboardPart-e8684ceb-eecf-4f48-9685-7294c16775be",
              "StartboardPart-LogsDashboardPart-e8684ceb-eecf-4f48-9685-7294c16775c0"
            ]
          }
        }
      }
    }
  }
}
