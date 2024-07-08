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
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        },
                        "name": "InstanceCount",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Automatic Scaling Instance Count",
                          "resourceDisplayName": "${timeseriessync_name}"
                        }
                      }
                    ],
                    "title": "Avg Automatic Scaling Instance Count for ${timeseriessync_name}",
                    "titleKind": 1,
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
        "7": {
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
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "customMetrics/ArchiveTimeSeriesMessageDuration",
                        "aggregationType": 4,
                        "namespace": "microsoft.insights/components/kusto",
                        "metricVisualization": {
                          "displayName": "ArchiveTimeSeriesMessageDuration"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "customMetrics/DequeueTimeSeriesMessageDuration",
                        "aggregationType": 4,
                        "namespace": "microsoft.insights/components/kusto",
                        "metricVisualization": {
                          "displayName": "DequeueTimeSeriesMessageDuration"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "customMetrics/SendTimeSeriesMessageDuration",
                        "aggregationType": 4,
                        "namespace": "microsoft.insights/components/kusto",
                        "metricVisualization": {
                          "displayName": "SendTimeSeriesMessageDuration"
                        }
                      }
                    ],
                    "title": "Avg ArchiveTimeSeriesMessageDuration, Avg DequeueTimeSeriesMessageDuration, and Avg SendTimeSeriesMessageDuration in milliseconds",
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
        "11": {
          "position": {
            "x": 17,
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
                        "name": "customMetrics/PeekedTimeSeriesMessageAge",
                        "aggregationType": 4,
                        "namespace": "microsoft.insights/components/kusto",
                        "metricVisualization": {
                          "displayName": "PeekedTimeSeriesMessageAge"
                        }
                      }
                    ],
                    "title": "Avg PeekedTimeSeriesMessageAge in seconds",
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
        "12": {
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
                "markdownSource": 1,
                "markdownUri": "",
                "subtitle": "",
                "title": ""
              }
            }
          }
        },
        "13": {
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
        "14": {
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
                          "displayName": "Count of dead-lettered messages in Topic.",
                          "resourceDisplayName": "${sbns_shared_name}"
                        }
                      }
                    ],
                    "title": "Dead-lettered messages in time series Service Bus Topic",
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
        "15": {
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
        "16": {
          "position": {
            "x": 17,
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
              "StartboardPart-MonitorChartPart-b45e38ad-1cde-454e-83fc-369e8e3d5674",
              "StartboardPart-MonitorChartPart-b45e38ad-1cde-454e-83fc-369e8e3d5676",
              "StartboardPart-MonitorChartPart-b45e38ad-1cde-454e-83fc-369e8e3d5678",
              "StartboardPart-MonitorChartPart-b45e38ad-1cde-454e-83fc-369e8e3d567a",
              "StartboardPart-MonitorChartPart-b45e38ad-1cde-454e-83fc-369e8e3d567c",
              "StartboardPart-MonitorChartPart-b45e38ad-1cde-454e-83fc-369e8e3d56ec",
              "StartboardPart-MonitorChartPart-b45e38ad-1cde-454e-83fc-369e8e3d5680",
              "StartboardPart-MonitorChartPart-b45e38ad-1cde-454e-83fc-369e8e3d5682",
              "StartboardPart-MonitorChartPart-b45e38ad-1cde-454e-83fc-369e8e3d5684",
              "StartboardPart-MonitorChartPart-b45e38ad-1cde-454e-83fc-369e8e3d5686",
              "StartboardPart-MonitorChartPart-b45e38ad-1cde-454e-83fc-369e8e3d568a",
              "StartboardPart-MonitorChartPart-b45e38ad-1cde-454e-83fc-369e8e3d568c",
              "StartboardPart-MonitorChartPart-b45e38ad-1cde-454e-83fc-369e8e3d568e",
              "StartboardPart-LogsDashboardPart-b45e38ad-1cde-454e-83fc-369e8e3d5690"
            ]
          }
        }
      }
    }
  }
}
