{
  "lenses": [
    {
      "order": 0,
      "parts": [
        {
          "position": {
            "x": 0,
            "y": 0,
            "colSpan": 7,
            "rowSpan": 1
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "content": "# Responsiveness",
                "title": "",
                "subtitle": "",
                "markdownSource": 1,
                "markdownUri": ""
              }
            }
          }
        },
        {
          "position": {
            "x": 7,
            "y": 0,
            "colSpan": 7,
            "rowSpan": 1
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "content": "# Reliability",
                "title": "",
                "subtitle": "",
                "markdownSource": 1,
                "markdownUri": ""
              }
            }
          }
        },
        {
          "position": {
            "x": 14,
            "y": 0,
            "colSpan": 7,
            "rowSpan": 1
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "content": "# Utilization ",
                "title": "",
                "subtitle": "",
                "markdownSource": 1,
                "markdownUri": ""
              }
            }
          }
        },
        {
          "position": {
            "x": 21,
            "y": 0,
            "colSpan": 7,
            "rowSpan": 1
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "content": "# Message generation ",
                "title": "",
                "subtitle": "",
                "markdownSource": 1,
                "markdownUri": ""
              }
            }
          }
        },
        {
          "position": {
            "x": 0,
            "y": 1,
            "colSpan": 7,
            "rowSpan": 4
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
                          "id": "${apim_sharedres_id}"
                        },
                        "name": "Requests",
                        "aggregationType": 1,
                        "namespace": "microsoft.apimanagement/service",
                        "metricVisualization": {
                          "displayName": "Requests"
                        }
                      }
                    ],
                    "title": "APIM - EDI B2B Request Amount per Response Status",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideHoverCard": false,
                        "hideLabelNames": true
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
                    "filterCollection": {
                      "filters": [
                        {
                          "key": "ApiId",
                          "operator": 0,
                          "values": [
                            "${apim_b2b_name}",
                            "${apim_b2b_ebix_name}"
                          ]
                        }
                      ]
                    },
                    "grouping": {
                      "dimension": [
                        "ApiId",
                        "BackendResponseCode"
                      ],
                      "sort": 2,
                      "top": 10
                    },
                    "timespan": {
                      "relative": {
                        "duration": 604800000
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
                          "id": "${apim_sharedres_id}"
                        },
                        "name": "Requests",
                        "aggregationType": 1,
                        "namespace": "microsoft.apimanagement/service",
                        "metricVisualization": {
                          "displayName": "Requests"
                        }
                      }
                    ],
                    "title": "B2B API Request Count by Response Status",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideHoverCard": false,
                        "hideLabelNames": true
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
                    },
                    "grouping": {
                      "dimension": "BackendResponseCode",
                      "sort": 2,
                      "top": 10
                    }
                  }
                }
              }
            },
            "filters": {
              "ApiId": {
                "model": {
                  "operator": "equals",
                  "values": [
                    "${apim_b2b_name}",
                    "${apim_b2b_ebix_name}"
                  ]
                }
              }
            }
          }
        },
        {
          "position": {
            "x": 7,
            "y": 1,
            "colSpan": 7,
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
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "EnqueueMessageAvgDuration",
                        "aggregationType": 4,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "EnqueueMessageAvgDuration"
                        }
                      }
                    ],
                    "title": "Avg Message Enqueue Time (µs)",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 1,
                      "legendVisualization": {
                        "hideHoverCard": false,
                        "hideLabelNames": true,
                        "isVisible": true,
                        "position": 2
                      },
                      "axisVisualization": {
                        "x": {
                          "axisType": 2,
                          "isVisible": true
                        },
                        "y": {
                          "axisType": 1,
                          "isVisible": true
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
        {
          "position": {
            "x": 14,
            "y": 1,
            "colSpan": 7,
            "rowSpan": 4
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
                          "id": "${sql_db_id}"
                        },
                        "name": "cpu_percent",
                        "aggregationType": 3,
                        "namespace": "microsoft.sql/servers/databases",
                        "metricVisualization": {
                          "displayName": "CPU percentage"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${sql_db_id}"
                        },
                        "name": "sql_instance_cpu_percent",
                        "aggregationType": 3,
                        "namespace": "microsoft.sql/servers/databases",
                        "metricVisualization": {
                          "displayName": "SQL instance CPU percent"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${sql_db_id}"
                        },
                        "name": "workers_percent",
                        "aggregationType": 3,
                        "namespace": "microsoft.sql/servers/databases",
                        "metricVisualization": {
                          "displayName": "Workers percentage"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${sql_db_id}"
                        },
                        "name": "storage_percent",
                        "aggregationType": 3,
                        "namespace": "microsoft.sql/servers/databases",
                        "metricVisualization": {
                          "displayName": "Data space used percent"
                        }
                      }
                    ],
                    "title": "Compute utilization",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideSubtitle": true,
                        "hideHoverCard": false,
                        "hideLabelNames": true
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
                        "duration": 259200000
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
                          "id": "${sql_db_id}"
                        },
                        "name": "cpu_percent",
                        "aggregationType": 3,
                        "namespace": "microsoft.sql/servers/databases",
                        "metricVisualization": {
                          "displayName": "CPU percentage"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${sql_db_id}"
                        },
                        "name": "sql_instance_cpu_percent",
                        "aggregationType": 3,
                        "namespace": "microsoft.sql/servers/databases",
                        "metricVisualization": {
                          "displayName": "SQL instance CPU percent"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${sql_db_id}"
                        },
                        "name": "workers_percent",
                        "aggregationType": 3,
                        "namespace": "microsoft.sql/servers/databases",
                        "metricVisualization": {
                          "displayName": "Workers percentage"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${sql_db_id}"
                        },
                        "name": "storage_percent",
                        "aggregationType": 3,
                        "namespace": "microsoft.sql/servers/databases",
                        "metricVisualization": {
                          "displayName": "Data space used percent"
                        }
                      }
                    ],
                    "title": "SQL Compute utilization",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideSubtitle": true,
                        "hideHoverCard": false,
                        "hideLabelNames": true
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
        {
          "position": {
            "x": 21,
            "y": 1,
            "colSpan": 7,
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
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "NotifyAggregatedMeasureDataJson",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "NotifyAggregatedMeasureDataJson"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "NotifyAggregatedMeasureDataResponseJson",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "NotifyAggregatedMeasureDataResponseJson"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "RejectRequestAggregatedMeasureDataJson",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "RejectRequestAggregatedMeasureDataJson"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "NotifyWholesaleServicesJson",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "NotifyWholesaleServicesJson"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "NotifyWholesaleServicesResponseJson",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "NotifyWholesaleServicesResponseJson"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "RejectRequestWholesaleSettlementJson",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "RejectRequestWholesaleSettlementJson"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "NotifyValidatedMeasureDataJson",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "NotifyValidatedMeasureDataJson"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "NotifyValidatedMeasureDataResponseJson",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "NotifyValidatedMeasureDataResponseJson"
                        }
                      }
                    ],
                    "title": "Number of new JSON messages",
                    "titleKind": 1,
                    "visualization": {
                      "chartType": 1,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideHoverCard": false,
                        "hideLabelNames": true
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
        {
          "position": {
            "x": 0,
            "y": 5,
            "colSpan": 7,
            "rowSpan": 4
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
                          "id": "${apim_sharedres_id}"
                        },
                        "name": "Duration",
                        "aggregationType": 4,
                        "namespace": "microsoft.apimanagement/service",
                        "metricVisualization": {
                          "displayName": "Overall Duration of Gateway Requests"
                        }
                      }
                    ],
                    "title": "APIM - EDI B2B Request Duration",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideHoverCard": false,
                        "hideLabelNames": true
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
                    "grouping": {
                      "dimension": "ApiId",
                      "sort": 2,
                      "top": 10
                    },
                    "timespan": {
                      "relative": {
                        "duration": 604800000
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
                          "id": "${apim_sharedres_id}"
                        },
                        "name": "Duration",
                        "aggregationType": 4,
                        "namespace": "microsoft.apimanagement/service",
                        "metricVisualization": {
                          "displayName": "Overall Duration of Gateway Requests"
                        }
                      }
                    ],
                    "title": "B2B API Request Duration",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "colorMap": {
                        "apima-bff-fe": "#637cef"
                      },
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideHoverCard": false,
                        "hideLabelNames": true
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
                    },
                    "grouping": {
                      "dimension": "ApiId",
                      "sort": 2,
                      "top": 10
                    }
                  }
                }
              }
            },
            "filters": {
              "ApiId": {
                "model": {
                  "operator": "equals",
                  "values": [
                    "${apim_b2b_name}",
                    "${apim_b2b_ebix_name}"
                  ]
                }
              }
            }
          }
        },
        {
          "position": {
            "x": 7,
            "y": 5,
            "colSpan": 7,
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
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "exceptions/count",
                        "aggregationType": 1,
                        "namespace": "microsoft.insights/components/kusto",
                        "metricVisualization": {
                          "displayName": "Exceptions"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "requests/failed",
                        "aggregationType": 1,
                        "namespace": "microsoft.insights/components/kusto",
                        "metricVisualization": {
                          "displayName": "Failed requests"
                        }
                      }
                    ],
                    "title": "Exceptions and Failed requests",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideHoverCard": false,
                        "hideLabelNames": true
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
            },
            "filters": {
              "cloud/roleName": {
                "model": {
                  "operator": "equals",
                  "values": [
                    "${func_b2b_name}"
                  ]
                }
              }
            }
          }
        },
        {
          "position": {
            "x": 14,
            "y": 5,
            "colSpan": 7,
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
                          "id": "${func_service_plan_id}"
                        },
                        "name": "CpuPercentage",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/serverfarms",
                        "metricVisualization": {
                          "displayName": "CPU Percentage"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${func_service_plan_id}"
                        },
                        "name": "MemoryPercentage",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/serverfarms",
                        "metricVisualization": {
                          "displayName": "Memory Percentage"
                        }
                      }
                    ],
                    "title": "B2B Compute utilization",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideHoverCard": false,
                        "hideLabelNames": true
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
        },{
          "position": {
            "x": 21,
            "y": 5,
            "colSpan": 7,
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
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "NotifyAggregatedMeasureDataXml",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "NotifyAggregatedMeasureDataXml"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "NotifyAggregatedMeasureDataResponseXml",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "NotifyAggregatedMeasureDataResponseXml"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "RejectRequestAggregatedMeasureDataXml",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "RejectRequestAggregatedMeasureDataXml"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "NotifyWholesaleServicesXml",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "NotifyWholesaleServicesXml"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "NotifyWholesaleServicesResponseXml",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "NotifyWholesaleServicesResponseXml"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "RejectRequestWholesaleSettlementXml",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "RejectRequestWholesaleSettlementXml"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "NotifyValidatedMeasureDataXml",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "NotifyValidatedMeasureDataXml"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "NotifyValidatedMeasureDataResponseXml",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "NotifyValidatedMeasureDataResponseXml"
                        }
                      }
                    ],
                    "title": "Number of new XML messages",
                    "titleKind": 1,
                    "visualization": {
                      "chartType": 1,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideHoverCard": false,
                        "hideLabelNames": true
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
        {
          "position": {
            "x": 0,
            "y": 9,
            "colSpan": 7,
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
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "DequeueRequestListener AvgDurationMs",
                        "aggregationType": 4,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "DequeueRequestListener AvgDurationMs"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "PeekRequestListener AvgDurationMs",
                        "aggregationType": 4,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "PeekRequestListener AvgDurationMs"
                        }
                      }
                    ],
                    "title": "Dequeue and Peek Request Listener Duration",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideHoverCard": false,
                        "hideLabelNames": true
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
        {
          "position": {
            "x": 7,
            "y": 9,
            "colSpan": 7,
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
                          "id": "${func_b2b_id}"
                        },
                        "name": "HealthCheckStatus",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Health check status"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${app_b2c_id}"
                        },
                        "name": "HealthCheckStatus",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Health check status"
                        }
                      }
                    ],
                    "title": "Health check status",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideHoverCard": false,
                        "hideLabelNames": true
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
        {
          "position": {
            "x": 14,
            "y": 9,
            "colSpan": 7,
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
                          "id": "${sql_db_id}"
                        },
                        "name": "sessions_count",
                        "aggregationType": 4,
                        "namespace": "microsoft.sql/servers/databases",
                        "metricVisualization": {
                          "displayName": "Sessions count"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${func_b2b_id}"
                        },
                        "name": "InstanceCount",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Automatic Scaling Instance Count"
                        }
                      }
                    ],
                    "title": "SQL Sessions count and B2B Scaling Count",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideHoverCard": false,
                        "hideLabelNames": true
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
        {
          "position": {
            "x": 21,
            "y": 9,
            "colSpan": 7,
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
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "NotifyAggregatedMeasureDataEbix",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "NotifyAggregatedMeasureDataEbix"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "NotifyAggregatedMeasureDataResponseEbix",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "NotifyAggregatedMeasureDataResponseEbix"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "RejectRequestAggregatedMeasureDataEbix",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "RejectRequestAggregatedMeasureDataEbix"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "NotifyWholesaleServicesEbix",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "NotifyWholesaleServicesEbix"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "NotifyWholesaleServicesResponseEbix",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "NotifyWholesaleServicesResponseEbix"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "RejectRequestWholesaleSettlementEbix",
                        "aggregationType": 7,
                        "namespace": "azure.applicationinsights",
                        "metricVisualization": {
                          "displayName": "RejectRequestWholesaleSettlementEbix"
                        }
                      }
                    ],
                    "title": "Number of new EBIX messages",
                    "titleKind": 1,
                    "visualization": {
                      "chartType": 1,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideHoverCard": false,
                        "hideLabelNames": true
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
      ]
    }
  ],
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
              "StartboardPart-MonitorChartPart-83c4f060-2ce3-4af7-83ce-3d74241f3215",
              "StartboardPart-MonitorChartPart-e8fc0a10-84c8-42cd-b2d4-f73efa54a085",
              "StartboardPart-MonitorChartPart-8f4c2d08-caf7-4521-b8af-427f5501a885",
              "StartboardPart-MonitorChartPart-83c4f060-2ce3-4af7-83ce-3d74241f31d8",
              "StartboardPart-MonitorChartPart-a1dbdafb-9f31-4176-8198-65691cabf0a6",
              "StartboardPart-MonitorChartPart-a7c1bbd5-a6be-41e5-a1c0-3e61723e4e95",
              "StartboardPart-MonitorChartPart-795bcdf1-4adc-4140-b114-761508817779",
              "StartboardPart-MonitorChartPart-dfb34afd-f19c-4aa6-af38-24ad67cb6206",
              "StartboardPart-MonitorChartPart-2a6c801b-2235-4b7a-9820-a8e81bdc62a4"
            ]
          }
        }
      }
    }
  }
}
