{
  "lenses": {
    "0": {
      "order": 0,
      "parts": {
        "0": {
          "position": {
            "x": 0,
            "y": 0,
            "colSpan": 2,
            "rowSpan": 8
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "content": "## General metrics",
                "title": "",
                "subtitle": "",
                "markdownSource": 1,
                "markdownUri": {}
              }
            }
          }
        },
        "1": {
          "position": {
            "x": 2,
            "y": 0,
            "colSpan": 5,
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
                          "id": "${timeseriessync_id}"
                        },
                        "name": "MemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Memory working set",
                          "resourceDisplayName": "${timeseriessync_name}"
                        }
                      }
                    ],
                    "title": "Avg Memory working set",
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
        "2": {
          "position": {
            "x": 7,
            "y": 0,
            "colSpan": 5,
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
                          "id": "${timeseriessync_id}"
                        },
                        "name": "Handles",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Handle Count",
                          "resourceDisplayName": "${timeseriessync_name}"
                        }
                      }
                    ],
                    "title": "Avg Handle Count",
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
        "3": {
          "position": {
            "x": 12,
            "y": 0,
            "colSpan": 5,
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
                          "id": "${timeseriessync_id}"
                        },
                        "name": "BytesReceived",
                        "aggregationType": 1,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Data In",
                          "resourceDisplayName": "${timeseriessync_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        },
                        "name": "BytesSent",
                        "aggregationType": 1,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Data Out",
                          "resourceDisplayName": "${timeseriessync_name}"
                        }
                      }
                    ],
                    "title": "Sum Data In/Out",
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
        "4": {
          "position": {
            "x": 2,
            "y": 4,
            "colSpan": 5,
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
                          "id": "${plan_services_id}"
                        },
                        "name": "MemoryPercentage",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/serverfarms",
                        "metricVisualization": {
                          "displayName": "Memory Percentage",
                          "resourceDisplayName": "${plan_services_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${plan_services_id}"
                        },
                        "name": "CpuPercentage",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/serverfarms",
                        "metricVisualization": {
                          "displayName": "CPU Percentage",
                          "resourceDisplayName": "${plan_services_name}"
                        }
                      }
                    ],
                    "title": "Avg Memory/CPU % Plan",
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
        "5": {
          "position": {
            "x": 7,
            "y": 4,
            "colSpan": 5,
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
                          "id": "${timeseriessync_id}"
                        },
                        "name": "HealthCheckStatus",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Health check status",
                          "resourceDisplayName": "${timeseriessync_name}"
                        }
                      }
                    ],
                    "title": "Avg Health check status",
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
        "6": {
          "position": {
            "x": 0,
            "y": 8,
            "colSpan": 2,
            "rowSpan": 4
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "content": "## Custom metrics",
                "title": "",
                "subtitle": "",
                "markdownSource": 1,
                "markdownUri": {}
              }
            }
          }
        },
        "7": {
          "position": {
            "x": 2,
            "y": 8,
            "colSpan": 5,
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
                        "name": "customMetrics/PeekedTimeSeriesMessageSize",
                        "aggregationType": 4,
                        "namespace": "microsoft.insights/components/kusto",
                        "metricVisualization": {
                          "displayName": "PeekedTimeSeriesMessageSize"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "customMetrics/PeekedTimeSeriesMessageSize",
                        "aggregationType": 2,
                        "namespace": "microsoft.insights/components/kusto",
                        "metricVisualization": {
                          "displayName": "PeekedTimeSeriesMessageSize"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "customMetrics/PeekedTimeSeriesMessageSize",
                        "aggregationType": 3,
                        "namespace": "microsoft.insights/components/kusto",
                        "metricVisualization": {
                          "displayName": "PeekedTimeSeriesMessageSize"
                        }
                      }
                    ],
                    "title": "Time series message size",
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
        "8": {
          "position": {
            "x": 7,
            "y": 8,
            "colSpan": 5,
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
                        "name": "customMetrics/TransformedTimeSeriesTransactions",
                        "aggregationType": 4,
                        "namespace": "microsoft.insights/components/kusto",
                        "metricVisualization": {
                          "displayName": "TransformedTimeSeriesTransactions"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "customMetrics/TransformedTimeSeriesTransactions",
                        "aggregationType": 2,
                        "namespace": "microsoft.insights/components/kusto",
                        "metricVisualization": {
                          "displayName": "TransformedTimeSeriesTransactions"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "customMetrics/TransformedTimeSeriesTransactions",
                        "aggregationType": 3,
                        "namespace": "microsoft.insights/components/kusto",
                        "metricVisualization": {
                          "displayName": "TransformedTimeSeriesTransactions"
                        }
                      }
                    ],
                    "title": "Transactions pr. message",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
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
        "9": {
          "position": {
            "x": 12,
            "y": 8,
            "colSpan": 5,
            "rowSpan": 4
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
                    "${appi_sharedres_id}"
                  ]
                },
                "isOptional": true
              },
              {
                "name": "PartId",
                "value": "8ab1ce32-1ab3-44b0-a279-3ecb8c538468",
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
                "isOptional": true
              },
              {
                "name": "Query",
                "value": "exceptions\n| where cloud_RoleName in (\"${timeseriessync_name}\")\n| summarize exceptionCount = count() by type\n| order by exceptionCount desc\n",
                "isOptional": true
              },
              {
                "name": "ControlType",
                "value": "FrameControlChart",
                "isOptional": true
              },
              {
                "name": "SpecificChart",
                "value": "StackedColumn",
                "isOptional": true
              },
              {
                "name": "PartTitle",
                "value": "Analytics",
                "isOptional": true
              },
              {
                "name": "PartSubTitle",
                "value": "${appi_sharedres_name}",
                "isOptional": true
              },
              {
                "name": "Dimensions",
                "value": {
                  "xAxis": {
                    "name": "type",
                    "type": "string"
                  },
                  "yAxis": [
                    {
                      "name": "exceptionCount",
                      "type": "long"
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
                  "isEnabled": true,
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
                "PartTitle": "Exceptions by type"
              }
            }
          }
        },
        "10": {
          "position": {
            "x": 0,
            "y": 12,
            "colSpan": 2,
            "rowSpan": 4
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "content": "## Failures",
                "title": "",
                "subtitle": "",
                "markdownSource": 1,
                "markdownUri": ""
              }
            }
          }
        },
        "11": {
          "position": {
            "x": 2,
            "y": 12,
            "colSpan": 5,
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
                          "id": "${timeseriessync_id}"
                        },
                        "name": "Http4xx",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Http 4xx",
                          "resourceDisplayName": "${timeseriessync_name}"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        },
                        "name": "Http5xx",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Http Server Errors",
                          "resourceDisplayName": "${timeseriessync_name}"
                        }
                      }
                    ],
                    "title": "Count Http 4xx and 5xx",
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
        "12": {
          "position": {
            "x": 7,
            "y": 12,
            "colSpan": 5,
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
                          "id": "${sbns_shared_id}"
                        },
                        "name": "DeadletteredMessages",
                        "aggregationType": 4,
                        "namespace": "microsoft.servicebus/namespaces",
                        "metricVisualization": {
                          "displayName": "Count of dead-lettered messages in a Queue/Topic.",
                          "resourceDisplayName": "${sbns_shared_name}"
                        }
                      }
                    ],
                    "title": "Dead-lettered messages in time series ServiceBus Queue",
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
              "EntityName": {
                "model": {
                  "operator": "equals",
                  "values": [
                    "sbq-mig-imported-time-series-messages"
                  ]
                }
              }
            }
          }
        },
        "13": {
          "position": {
            "x": 12,
            "y": 12,
            "colSpan": 5,
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
                        "aggregationType": 7,
                        "namespace": "microsoft.insights/components",
                        "metricVisualization": {
                          "displayName": "Exceptions",
                          "resourceDisplayName": "${appi_sharedres_name}"
                        }
                      }
                    ],
                    "title": "Count Exceptions in TimeSeriesSynchronization App",
                    "titleKind": 2,
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
            },
            "filters": {
              "cloud/roleName": {
                "model": {
                  "operator": "equals",
                  "values": [
                    "${timeseriessync_name}"
                  ]
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
              "StartboardPart-MonitorChartPart-9e5bff81-3a06-4f11-884d-d46dfa203125",
              "StartboardPart-MonitorChartPart-9e5bff81-3a06-4f11-884d-d46dfa203127",
              "StartboardPart-MonitorChartPart-9e5bff81-3a06-4f11-884d-d46dfa203129",
              "StartboardPart-MonitorChartPart-9e5bff81-3a06-4f11-884d-d46dfa20312b",
              "StartboardPart-MonitorChartPart-9e5bff81-3a06-4f11-884d-d46dfa20312d",
              "StartboardPart-MonitorChartPart-9e5bff81-3a06-4f11-884d-d46dfa203131",
              "StartboardPart-MonitorChartPart-9e5bff81-3a06-4f11-884d-d46dfa203133",
              "StartboardPart-LogsDashboardPart-9e5bff81-3a06-4f11-884d-d46dfa203135",
              "StartboardPart-MonitorChartPart-9e5bff81-3a06-4f11-884d-d46dfa203139",
              "StartboardPart-MonitorChartPart-9e5bff81-3a06-4f11-884d-d46dfa20313b",
              "StartboardPart-MonitorChartPart-9e5bff81-3a06-4f11-884d-d46dfa20313d"
            ]
          }
        }
      }
    }
  }
}