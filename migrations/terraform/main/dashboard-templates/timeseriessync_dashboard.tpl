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
                "markdownSource": 1,
                "markdownUri": {},
                "subtitle": "",
                "title": ""
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
                        "aggregationType": 4,
                        "metricVisualization": {
                          "displayName": "Memory working set",
                          "resourceDisplayName": "${timeseriessync_name}"
                        },
                        "name": "MemoryWorkingSet",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        }
                      }
                    ],
                    "title": "Avg Memory working set",
                    "titleKind": 2,
                    "visualization": {
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
                      "chartType": 2,
                      "disablePinning": true,
                      "legendVisualization": {
                        "hideHoverCard": false,
                        "hideLabelNames": true,
                        "isVisible": true,
                        "position": 2
                      }
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
                        "aggregationType": 4,
                        "metricVisualization": {
                          "displayName": "Handle Count",
                          "resourceDisplayName": "${timeseriessync_name}"
                        },
                        "name": "Handles",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        }
                      }
                    ],
                    "title": "Avg Handle Count",
                    "titleKind": 2,
                    "visualization": {
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
                      "chartType": 2,
                      "disablePinning": true,
                      "legendVisualization": {
                        "hideHoverCard": false,
                        "hideLabelNames": true,
                        "isVisible": true,
                        "position": 2
                      }
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
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Data In",
                          "resourceDisplayName": "${timeseriessync_name}"
                        },
                        "name": "BytesReceived",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Data Out",
                          "resourceDisplayName": "${timeseriessync_name}"
                        },
                        "name": "BytesSent",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        }
                      }
                    ],
                    "title": "Sum Data In/Out",
                    "titleKind": 2,
                    "visualization": {
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
                      "chartType": 2,
                      "disablePinning": true,
                      "legendVisualization": {
                        "hideHoverCard": false,
                        "hideLabelNames": true,
                        "isVisible": true,
                        "position": 2
                      }
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
                        "aggregationType": 4,
                        "metricVisualization": {
                          "displayName": "Memory Percentage",
                          "resourceDisplayName": "${plan_services_name}"
                        },
                        "name": "MemoryPercentage",
                        "namespace": "microsoft.web/serverfarms",
                        "resourceMetadata": {
                          "id": "${plan_services_id}"
                        }
                      },
                      {
                        "aggregationType": 4,
                        "metricVisualization": {
                          "displayName": "CPU Percentage",
                          "resourceDisplayName": "${plan_services_name}"
                        },
                        "name": "CpuPercentage",
                        "namespace": "microsoft.web/serverfarms",
                        "resourceMetadata": {
                          "id": "${plan_services_id}"
                        }
                      }
                    ],
                    "title": "Avg Memory/CPU % Plan",
                    "titleKind": 2,
                    "visualization": {
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
                      "chartType": 2,
                      "disablePinning": true,
                      "legendVisualization": {
                        "hideHoverCard": false,
                        "hideLabelNames": true,
                        "isVisible": true,
                        "position": 2
                      }
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
                        "aggregationType": 4,
                        "metricVisualization": {
                          "displayName": "Health check status",
                          "resourceDisplayName": "${timeseriessync_name}"
                        },
                        "name": "HealthCheckStatus",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        }
                      }
                    ],
                    "title": "Avg Health check status",
                    "titleKind": 2,
                    "visualization": {
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
                      "chartType": 2,
                      "disablePinning": true,
                      "legendVisualization": {
                        "hideHoverCard": false,
                        "hideLabelNames": true,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "6": {
          "position": {
            "x": 12,
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
                        "aggregationType": 4,
                        "metricVisualization": {
                          "displayName": "Automatic Scaling Instance Count",
                          "resourceDisplayName": "${timeseriessync_name}"
                        },
                        "name": "InstanceCount",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        }
                      }
                    ],
                    "title": "Avg Automatic Scaling Instance Count for ${timeseriessync_name}",
                    "titleKind": 1,
                    "visualization": {
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
                      "chartType": 2,
                      "disablePinning": true,
                      "legendVisualization": {
                        "hideHoverCard": false,
                        "hideLabelNames": true,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "7": {
          "position": {
            "x": 0,
            "y": 8,
            "colSpan": 2,
            "rowSpan": 8
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "content": "## Custom metrics",
                "markdownSource": 1,
                "markdownUri": {},
                "subtitle": "",
                "title": ""
              }
            }
          }
        },
        "8": {
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
                        "aggregationType": 4,
                        "metricVisualization": {
                          "displayName": "PeekedTimeSeriesMessageSize"
                        },
                        "name": "customMetrics/PeekedTimeSeriesMessageSize",
                        "namespace": "microsoft.insights/components/kusto",
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        }
                      },
                      {
                        "aggregationType": 2,
                        "metricVisualization": {
                          "displayName": "PeekedTimeSeriesMessageSize"
                        },
                        "name": "customMetrics/PeekedTimeSeriesMessageSize",
                        "namespace": "microsoft.insights/components/kusto",
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        }
                      },
                      {
                        "aggregationType": 3,
                        "metricVisualization": {
                          "displayName": "PeekedTimeSeriesMessageSize"
                        },
                        "name": "customMetrics/PeekedTimeSeriesMessageSize",
                        "namespace": "microsoft.insights/components/kusto",
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        }
                      }
                    ],
                    "title": "Time series message size",
                    "titleKind": 2,
                    "visualization": {
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
                      "chartType": 2,
                      "disablePinning": true,
                      "legendVisualization": {
                        "hideHoverCard": false,
                        "hideLabelNames": true,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "9": {
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
                        "aggregationType": 4,
                        "metricVisualization": {
                          "displayName": "TransformedTimeSeriesTransactions"
                        },
                        "name": "customMetrics/TransformedTimeSeriesTransactions",
                        "namespace": "microsoft.insights/components/kusto",
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        }
                      },
                      {
                        "aggregationType": 2,
                        "metricVisualization": {
                          "displayName": "TransformedTimeSeriesTransactions"
                        },
                        "name": "customMetrics/TransformedTimeSeriesTransactions",
                        "namespace": "microsoft.insights/components/kusto",
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        }
                      },
                      {
                        "aggregationType": 3,
                        "metricVisualization": {
                          "displayName": "TransformedTimeSeriesTransactions"
                        },
                        "name": "customMetrics/TransformedTimeSeriesTransactions",
                        "namespace": "microsoft.insights/components/kusto",
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        }
                      }
                    ],
                    "title": "Transactions pr. message",
                    "titleKind": 2,
                    "visualization": {
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
                      "chartType": 2,
                      "disablePinning": true,
                      "legendVisualization": {
                        "hideHoverCard": false,
                        "hideLabelNames": true,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "10": {
          "position": {
            "x": 12,
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
                        "aggregationType": 4,
                        "metricVisualization": {
                          "displayName": "ArchiveTimeSeriesMessageDuration"
                        },
                        "name": "customMetrics/ArchiveTimeSeriesMessageDuration",
                        "namespace": "microsoft.insights/components/kusto",
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        }
                      },
                      {
                        "aggregationType": 4,
                        "metricVisualization": {
                          "displayName": "DequeueTimeSeriesMessageDuration"
                        },
                        "name": "customMetrics/DequeueTimeSeriesMessageDuration",
                        "namespace": "microsoft.insights/components/kusto",
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        }
                      },
                      {
                        "aggregationType": 4,
                        "metricVisualization": {
                          "displayName": "SendTimeSeriesMessageDuration"
                        },
                        "name": "customMetrics/SendTimeSeriesMessageDuration",
                        "namespace": "microsoft.insights/components/kusto",
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        }
                      }
                    ],
                    "title": "Avg ArchiveTimeSeriesMessageDuration, Avg DequeueTimeSeriesMessageDuration, and Avg SendTimeSeriesMessageDuration in milliseconds",
                    "titleKind": 2,
                    "visualization": {
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
                      "chartType": 2,
                      "disablePinning": true,
                      "legendVisualization": {
                        "hideHoverCard": false,
                        "hideLabelNames": true,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                }
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
                "value": "2e318f26-5d12-40b8-9666-ab2293108efa",
                "isOptional": true
              },
              {
                "name": "Version",
                "value": "2.0",
                "isOptional": true
              },
              {
                "name": "TimeRange",
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
                "name": "ControlType",
                "value": "FrameControlChart",
                "isOptional": true
              },
              {
                "name": "SpecificChart",
                "value": "Line",
                "isOptional": true
              },
              {
                "name": "PartTitle",
                "value": "Analytics",
                "isOptional": true
              },
              {
                "name": "Dimensions",
                "value": {
                  "xAxis": {
                    "name": "timestamp",
                    "type": "datetime"
                  },
                  "yAxis": [
                    {
                      "name": "Count",
                      "type": "real"
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
                "value": true,
                "isOptional": true
              }
            ],
            "type": "Extension/Microsoft_OperationsManagementSuite_Workspace/PartType/LogsDashboardPart",
            "settings": {
              "content": {
                "Query": "traces\n| where cloud_RoleName in (\"${timeseriessync_name}\")\n| where message contains \"Reprocessing TimeSeriesIntermediaryBlob with id\"\n| project timestamp, itemCount;\n\n",
                "ControlType": "AnalyticsGrid",
                "PartTitle": "Blobs reprocessed from Intermediary",
                "IsQueryContainTimeRange": false
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
                        "aggregationType": 4,
                        "metricVisualization": {
                          "displayName": "PeekedTimeSeriesMessageAge"
                        },
                        "name": "customMetrics/PeekedTimeSeriesMessageAge",
                        "namespace": "microsoft.insights/components/kusto",
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        }
                      }
                    ],
                    "title": "Avg PeekedTimeSeriesMessageAge in seconds",
                    "titleKind": 2,
                    "visualization": {
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
                      "chartType": 2,
                      "disablePinning": true,
                      "legendVisualization": {
                        "hideHoverCard": false,
                        "hideLabelNames": true,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "13": {
          "position": {
            "x": 0,
            "y": 16,
            "colSpan": 2,
            "rowSpan": 4
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "content": "## Failures",
                "markdownSource": 1,
                "markdownUri": "",
                "subtitle": "",
                "title": ""
              }
            }
          }
        },
        "14": {
          "position": {
            "x": 2,
            "y": 16,
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
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Http 4xx",
                          "resourceDisplayName": "${timeseriessync_name}"
                        },
                        "name": "Http4xx",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        }
                      },
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Http Server Errors",
                          "resourceDisplayName": "${timeseriessync_name}"
                        },
                        "name": "Http5xx",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        }
                      }
                    ],
                    "title": "Count Http 4xx and 5xx",
                    "titleKind": 2,
                    "visualization": {
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
                      "chartType": 2,
                      "disablePinning": true,
                      "legendVisualization": {
                        "hideHoverCard": false,
                        "hideLabelNames": true,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "15": {
          "position": {
            "x": 7,
            "y": 16,
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
                        "aggregationType": 4,
                        "metricVisualization": {
                          "displayName": "Count of dead-lettered messages in Topic.",
                          "resourceDisplayName": "${sbns_shared_name}"
                        },
                        "name": "DeadletteredMessages",
                        "namespace": "microsoft.servicebus/namespaces",
                        "resourceMetadata": {
                          "id": "${sbns_shared_name}"
                        }
                      }
                    ],
                    "title": "Dead-lettered messages in time series Service Bus Topic",
                    "titleKind": 2,
                    "visualization": {
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
                      "chartType": 2,
                      "disablePinning": true,
                      "legendVisualization": {
                        "hideHoverCard": false,
                        "hideLabelNames": true,
                        "isVisible": true,
                        "position": 2
                      }
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
                    "sbt-mig-imported-time-series-messages"
                  ]
                }
              }
            }
          }
        },
        "16": {
          "position": {
            "x": 12,
            "y": 16,
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
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Exceptions",
                          "resourceDisplayName": "${appi_sharedres_name}"
                        },
                        "name": "exceptions/count",
                        "namespace": "microsoft.insights/components",
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        }
                      }
                    ],
                    "title": "Count Exceptions in TimeSeriesSynchronization App",
                    "titleKind": 2,
                    "visualization": {
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
                      "chartType": 1,
                      "disablePinning": true,
                      "legendVisualization": {
                        "hideHoverCard": false,
                        "hideLabelNames": true,
                        "isVisible": true,
                        "position": 2
                      }
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
        },
        "17": {
          "position": {
            "x": 17,
            "y": 16,
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
                  "aggregation": "Sum",
                  "splitBy": [],
                  "xAxis": {
                    "name": "type",
                    "type": "string"
                  },
                  "yAxis": [
                    {
                      "name": "exceptionCount",
                      "type": "long"
                    }
                  ]
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
              "StartboardPart-MonitorChartPart-b399bb4e-e921-4588-8dae-98ae55263b4b",
              "StartboardPart-MonitorChartPart-b399bb4e-e921-4588-8dae-98ae55263b5b",
              "StartboardPart-MonitorChartPart-b399bb4e-e921-4588-8dae-98ae55263b5d",
              "StartboardPart-MonitorChartPart-b399bb4e-e921-4588-8dae-98ae55263bd7",
              "StartboardPart-LogsDashboardPart-b399bb4e-e921-4588-8dae-98ae55263e40",
              "StartboardPart-MonitorChartPart-b399bb4e-e921-4588-8dae-98ae55263b5f",
              "StartboardPart-MonitorChartPart-b399bb4e-e921-4588-8dae-98ae55263b61",
              "StartboardPart-MonitorChartPart-b399bb4e-e921-4588-8dae-98ae55263b63",
              "StartboardPart-MonitorChartPart-b399bb4e-e921-4588-8dae-98ae55263b67",
              "StartboardPart-MonitorChartPart-b399bb4e-e921-4588-8dae-98ae55263b69",
              "StartboardPart-MonitorChartPart-b399bb4e-e921-4588-8dae-98ae55263b4d",
              "StartboardPart-MonitorChartPart-b399bb4e-e921-4588-8dae-98ae55263b4f",
              "StartboardPart-MonitorChartPart-b399bb4e-e921-4588-8dae-98ae55263b53",
              "StartboardPart-MonitorChartPart-b399bb4e-e921-4588-8dae-98ae55263b55",
              "StartboardPart-MonitorChartPart-b399bb4e-e921-4588-8dae-98ae55263b57",
              "StartboardPart-LogsDashboardPart-b399bb4e-e921-4588-8dae-98ae55263b59"
            ]
          }
        }
      }
    }
  }
}
