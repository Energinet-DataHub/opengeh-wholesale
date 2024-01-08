{
  "lenses": {
    "0": {
      "order": 0,
      "parts": {
        "0": {
          "position": {
            "x": 0,
            "y": 0,
            "colSpan": 3,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "settings": {
                  "content": "### App performance",
                  "markdownSource": 1,
                  "markdownUri": null,
                  "subtitle": "",
                  "title": ""
                }
              }
            }
          }
        },
        "1": {
          "position": {
            "x": 3,
            "y": 0,
            "colSpan": 4,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "value": {
                  "chart": {
                    "metrics": [
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Average memory working set"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "timespan": {
                      "grain": 1,
                      "relative": {
                        "duration": 86400000
                      },
                      "showUTCTime": false
                    },
                    "title": "Average memory usage over time",
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
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                },
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
                          "id": "${timeseriesapi_id}"
                        },
                        "name": "MemoryWorkingSet",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Memory working set"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        },
                        "name": "MemoryWorkingSet",
                        "aggregationType": 3,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Memory working set"
                        }
                      }
                    ],
                    "title": "Average memory usage over time",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "hideSubtitle": false,
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
        "2": {
          "position": {
            "x": 7,
            "y": 0,
            "colSpan": 4,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "value": {
                  "chart": {
                    "metrics": [
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "CPU Time"
                        },
                        "name": "CpuTime",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "timespan": {
                      "grain": 1,
                      "relative": {
                        "duration": 86400000
                      },
                      "showUTCTime": false
                    },
                    "title": "Average memory usage over time",
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
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                },
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
                          "id": "${timeseriesapi_id}"
                        },
                        "name": "CpuTime",
                        "aggregationType": 1,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "CPU Time"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        },
                        "name": "CpuTime",
                        "aggregationType": 3,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "CPU Time"
                        }
                      }
                    ],
                    "title": "CPU consumption",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "hideSubtitle": false,
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
        "3": {
          "position": {
            "x": 11,
            "y": 0,
            "colSpan": 4,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "value": {
                  "chart": {
                    "metrics": [
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Thread Count"
                        },
                        "name": "Threads",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "timespan": {
                      "grain": 1,
                      "relative": {
                        "duration": 2592000000
                      },
                      "showUTCTime": false
                    },
                    "title": "Thread count",
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
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                },
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
                          "displayName": "Thread Count"
                        },
                        "name": "Threads",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "title": "Thread count",
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
                        "hideSubtitle": false,
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
            "x": 15,
            "y": 0,
            "colSpan": 4,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "value": {
                  "chart": {
                    "metrics": [
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Handle Count"
                        },
                        "name": "Handles",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "timespan": {
                      "grain": 1,
                      "relative": {
                        "duration": 86400000
                      },
                      "showUTCTime": false
                    },
                    "title": "Sum Handle Count",
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
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                },
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
                          "displayName": "Handle Count"
                        },
                        "name": "Handles",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "title": "Sum Handle Count",
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
                        "hideSubtitle": false,
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
            "x": 19,
            "y": 0,
            "colSpan": 4,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "value": {
                  "chart": {
                    "metrics": [
                      {
                        "aggregationType": 4,
                        "metricVisualization": {
                          "displayName": "Health check status"
                        },
                        "name": "HealthCheckStatus",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "timespan": {
                      "grain": 1,
                      "relative": {
                        "duration": 86400000
                      },
                      "showUTCTime": false
                    },
                    "titleKind": 0,
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
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                },
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
                          "displayName": "Health check status"
                        },
                        "name": "HealthCheckStatus",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "title": "Health check status",
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
                        "hideSubtitle": false,
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
            "x": 0,
            "y": 3,
            "colSpan": 3,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "settings": {
                  "content": "### Usage",
                  "markdownSource": 1,
                  "markdownUri": null,
                  "subtitle": "",
                  "title": ""
                }
              }
            }
          }
        },
        "7": {
          "position": {
            "x": 3,
            "y": 3,
            "colSpan": 4,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "value": {
                  "chart": {
                    "metrics": [
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Response Time"
                        },
                        "name": "HttpResponseTime",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "timespan": {
                      "grain": 1,
                      "relative": {
                        "duration": 2592000000
                      },
                      "showUTCTime": false
                    },
                    "title": "Response time",
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
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                },
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
                          "displayName": "Response Time"
                        },
                        "name": "HttpResponseTime",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "title": "Response time (Avg)",
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
                        "hideSubtitle": false,
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
        "8": {
          "position": {
            "x": 7,
            "y": 3,
            "colSpan": 4,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "value": {
                  "chart": {
                    "metrics": [
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Response Time"
                        },
                        "name": "HttpResponseTime",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "timespan": {
                      "grain": 1,
                      "relative": {
                        "duration": 2592000000
                      },
                      "showUTCTime": false
                    },
                    "title": "Response time",
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
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                },
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
                        "aggregationType": 2,
                        "metricVisualization": {
                          "displayName": "Response Time"
                        },
                        "name": "HttpResponseTime",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      },
                      {
                        "aggregationType": 3,
                        "metricVisualization": {
                          "displayName": "Response Time"
                        },
                        "name": "HttpResponseTime",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "title": "Response time (Min/Max)",
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
                        "hideSubtitle": false,
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
            "x": 11,
            "y": 3,
            "colSpan": 4,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "value": {
                  "chart": {
                    "metrics": [
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Requests"
                        },
                        "name": "Requests",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "timespan": {
                      "grain": 1,
                      "relative": {
                        "duration": 2592000000
                      },
                      "showUTCTime": false
                    },
                    "title": "Total requests",
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
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                },
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
                          "displayName": "Requests"
                        },
                        "name": "Requests",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "title": "Total requests",
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
                        "hideSubtitle": false,
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
            "x": 15,
            "y": 3,
            "colSpan": 6,
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
                    "${appi_sharedres_id}"
                  ]
                },
                "isOptional": true
              },
              {
                "name": "PartId",
                "value": "118ae4a0-1130-4205-8201-3e7ae64e85b3",
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
                "value": "requests\n| where cloud_RoleName == \"${timeseriesapi_name}\"\n| summarize p95_response_time = percentile(duration, 95), p99_response_time = percentile(duration, 99) by bin(timestamp, 1d)\n\n",
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
                "value": "${appi_sharedres_name}",
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
                "PartTitle": "Response time percentiles"
              }
            }
          }
        },
        "11": {
          "position": {
            "x": 0,
            "y": 6,
            "colSpan": 3,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "settings": {
                  "content": "### Http codes",
                  "markdownSource": 1,
                  "markdownUri": null,
                  "subtitle": "",
                  "title": ""
                }
              }
            }
          }
        },
        "12": {
          "position": {
            "x": 3,
            "y": 6,
            "colSpan": 6,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "value": {
                  "chart": {
                    "metrics": [
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Http Server Errors"
                        },
                        "name": "Http5xx",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "timespan": {
                      "grain": 1,
                      "relative": {
                        "duration": 2592000000
                      },
                      "showUTCTime": false
                    },
                    "title": "Count of Http server errors",
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
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                },
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
                          "displayName": "Http Server Errors"
                        },
                        "name": "Http5xx",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "title": "Count of Http server errors",
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
                        "hideSubtitle": false,
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
            "x": 9,
            "y": 6,
            "colSpan": 6,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "value": {
                  "chart": {
                    "metrics": [
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Http 401"
                        },
                        "name": "Http401",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      },
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Http 403"
                        },
                        "name": "Http403",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      },
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Http 404"
                        },
                        "name": "Http404",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      },
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Http 406"
                        },
                        "name": "Http406",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      },
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Http 2xx"
                        },
                        "name": "Http2xx",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "timespan": {
                      "grain": 1,
                      "relative": {
                        "duration": 2592000000
                      },
                      "showUTCTime": false
                    },
                    "title": "Count of Http status codes",
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
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                },
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
                          "displayName": "Http 401"
                        },
                        "name": "Http401",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      },
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Http 403"
                        },
                        "name": "Http403",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      },
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Http 404"
                        },
                        "name": "Http404",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      },
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Http 406"
                        },
                        "name": "Http406",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      },
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Http 2xx"
                        },
                        "name": "Http2xx",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "title": "Count of Http status codes",
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
                        "hideSubtitle": false,
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
            "x": 0,
            "y": 9,
            "colSpan": 3,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "content": "### Failures",
                "markdownSource": 1,
                "markdownUri": "",
                "subtitle": "",
                "title": ""
              }
            }
          }
        },
        "15": {
          "position": {
            "x": 3,
            "y": 9,
            "colSpan": 5,
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
                    "filterCollection": {
                      "filters": [
                        {
                          "key": "cloud/roleName",
                          "operator": 0,
                          "values": [
                            "${timeseriesapi_name}"
                          ]
                        }
                      ]
                    },
                    "metrics": [
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Exceptions"
                        },
                        "name": "exceptions/count",
                        "namespace": "microsoft.insights/components/kusto",
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        }
                      }
                    ],
                    "timespan": {
                      "grain": 1,
                      "relative": {
                        "duration": 86400000
                      },
                      "showUTCTime": false
                    },
                    "title": "Sum Exceptions",
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
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2
                      }
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
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Exceptions"
                        },
                        "name": "exceptions/count",
                        "namespace": "microsoft.insights/components/kusto",
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        }
                      }
                    ],
                    "title": "Sum of Exceptions",
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
                        "hideSubtitle": false,
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
                    "${timeseriesapi_name}"
                  ]
                }
              }
            }
          }
        },
        "16": {
          "position": {
            "x": 9,
            "y": 9,
            "colSpan": 5,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "value": {
                  "chart": {
                    "filterCollection": {
                      "filters": [
                        {
                          "key": "cloud/roleName",
                          "operator": 0,
                          "values": [
                            "${timeseriesapi_name}"
                          ]
                        }
                      ]
                    },
                    "metrics": [
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Exceptions"
                        },
                        "name": "exceptions/count",
                        "namespace": "microsoft.insights/components/kusto",
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        }
                      }
                    ],
                    "timespan": {
                      "grain": 1,
                      "relative": {
                        "duration": 86400000
                      },
                      "showUTCTime": false
                    },
                    "title": "Sum of Exceptions",
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
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                },
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
                          "displayName": "Traces"
                        },
                        "name": "traces/count",
                        "namespace": "microsoft.insights/components/kusto",
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        }
                      }
                    ],
                    "title": "Sum Traces with Error Severity",
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
                        "hideSubtitle": false,
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
                    "${timeseriesapi_name}"
                  ]
                }
              },
              "trace/severityLevel": {
                "model": {
                  "operator": "equals",
                  "values": [
                    "3"
                  ]
                }
              }
            }
          }
        },
        "17": {
          "position": {
            "x": 15,
            "y": 9,
            "colSpan": 6,
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
                    "${appi_sharedres_id}"
                  ]
                },
                "isOptional": true
              },
              {
                "name": "PartId",
                "value": "15dd7818-94df-4f49-bb13-324ccf05daa4",
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
                "isOptional": true
              },
              {
                "name": "Query",
                "value": "exceptions\n| where cloud_RoleName in (\"${timeseriesapi_name}\")\n| summarize exceptionCount = count() by type\n| order by exceptionCount desc\n",
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
                "value": "${appi_sharedres_name}",
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
                "PartTitle": "Exception Overview"
              }
            }
          }
        },
        "18": {
          "position": {
            "x": 8,
            "y": 11,
            "colSpan": 1,
            "rowSpan": 1
          },
          "metadata": {
            "inputs": [
              {
                "name": "ResourceId",
                "value": "${appi_sharedres_id}",
                "isOptional": true
              },
              {
                "name": "FilterSpec",
                "value": {
                  "filterWhereClause": "| where cloud_RoleName in (\"${timeseriesapi_name}\")| order by timestamp desc",
                  "originalParams": {
                    "eventTypes": [
                      {
                        "label": "Exception",
                        "tableName": "exceptions",
                        "value": "exception"
                      }
                    ],
                    "filter": [
                      {
                        "dimension": {
                          "displayName": "Cloud role name",
                          "draftKey": "cloud/roleName",
                          "name": "cloud/roleName",
                          "tables": [
                            "availabilityResults",
                            "requests",
                            "exceptions",
                            "pageViews",
                            "traces",
                            "customEvents",
                            "dependencies"
                          ]
                        },
                        "operator": {
                          "isSelected": true,
                          "label": "=",
                          "value": "=="
                        },
                        "values": [
                          "${timeseriesapi_name}"
                        ]
                      }
                    ],
                    "searchPhrase": {
                      "_tokens": [],
                      "originalPhrase": ""
                    },
                    "sort": "desc",
                    "timeContext": {
                      "durationMs": 86400000
                    },
                    "timeOptions": {
                      "appliedISOGrain": "Auto",
                      "showUTCTime": false
                    }
                  },
                  "tables": [
                    "exceptions"
                  ],
                  "timeContextWhereClause": "| where timestamp > datetime(\"2023-12-28T10:03:53.636Z\") and timestamp < datetime(\"2023-12-29T10:03:53.636Z\")"
                },
                "isOptional": true
              }
            ],
            "type": "Extension/AppInsightsExtension/PartType/SearchV2PinnedPart",
            "deepLink": "#@energinet.onmicrosoft.com/resource${appi_sharedres_id}/searchV1"
          }
        },
        "19": {
          "position": {
            "x": 14,
            "y": 11,
            "colSpan": 1,
            "rowSpan": 1
          },
          "metadata": {
            "inputs": [
              {
                "name": "ResourceId",
                "value": "${appi_sharedres_id}",
                "isOptional": true
              },
              {
                "name": "FilterSpec",
                "value": {
                  "filterWhereClause": "| where cloud_RoleName in (\"${timeseriesapi_name}\")| where severityLevel in (\"3\")| order by timestamp desc",
                  "originalParams": {
                    "eventTypes": [
                      {
                        "label": "Trace",
                        "tableName": "traces",
                        "value": "trace"
                      }
                    ],
                    "filter": [
                      {
                        "dimension": {
                          "displayName": "Cloud role name",
                          "draftKey": "cloud/roleName",
                          "name": "cloud/roleName",
                          "tables": [
                            "availabilityResults",
                            "requests",
                            "exceptions",
                            "pageViews",
                            "traces",
                            "customEvents",
                            "dependencies"
                          ]
                        },
                        "operator": {
                          "isSelected": true,
                          "label": "=",
                          "value": "=="
                        },
                        "values": [
                          "${timeseriesapi_name}"
                        ]
                      },
                      {
                        "dimension": {
                          "displayName": "Trace Severity level",
                          "draftKey": "trace/severityLevel",
                          "name": "trace/severityLevel",
                          "tables": [
                            "traces"
                          ]
                        },
                        "operator": {
                          "isSelected": true,
                          "label": "=",
                          "value": "=="
                        },
                        "values": [
                          "3"
                        ]
                      }
                    ],
                    "searchPhrase": {
                      "_tokens": [],
                      "originalPhrase": ""
                    },
                    "sort": "desc",
                    "timeContext": {
                      "durationMs": 86400000
                    },
                    "timeOptions": {
                      "appliedISOGrain": "Auto",
                      "showUTCTime": false
                    }
                  },
                  "tables": [
                    "traces"
                  ],
                  "timeContextWhereClause": "| where timestamp > datetime(\"2023-12-28T10:03:23.972Z\") and timestamp < datetime(\"2023-12-29T10:03:23.972Z\")"
                },
                "isOptional": true
              }
            ],
            "type": "Extension/AppInsightsExtension/PartType/SearchV2PinnedPart",
            "deepLink": "#@energinet.onmicrosoft.com/resource${appi_sharedres_id}/searchV1"
          }
        },
        "20": {
          "position": {
            "x": 0,
            "y": 12,
            "colSpan": 3,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "settings": {
                  "content": "### Bandwidth",
                  "markdownSource": 1,
                  "markdownUri": null,
                  "subtitle": "",
                  "title": ""
                }
              }
            }
          }
        },
        "21": {
          "position": {
            "x": 3,
            "y": 12,
            "colSpan": 6,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "value": {
                  "chart": {
                    "metrics": [
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Data Out"
                        },
                        "name": "BytesSent",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "timespan": {
                      "grain": 1,
                      "relative": {
                        "duration": 2592000000
                      },
                      "showUTCTime": false
                    },
                    "title": "Outgoing bandwidth",
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
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                },
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
                          "displayName": "Data Out"
                        },
                        "name": "BytesSent",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "title": "Outgoing bandwidth",
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
                        "hideSubtitle": false,
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
        "22": {
          "position": {
            "x": 9,
            "y": 12,
            "colSpan": 6,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [
              {
                "name": "options",
                "value": {
                  "chart": {
                    "metrics": [
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Data In"
                        },
                        "name": "BytesReceived",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "timespan": {
                      "grain": 1,
                      "relative": {
                        "duration": 2592000000
                      },
                      "showUTCTime": false
                    },
                    "title": "Incoming bandwidth",
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
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2
                      }
                    }
                  }
                },
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
                          "displayName": "Data In"
                        },
                        "name": "BytesReceived",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${timeseriesapi_id}"
                        }
                      }
                    ],
                    "title": "Incoming bandwidth",
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
                        "hideSubtitle": false,
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
        "23": {
          "position": {
            "x": 0,
            "y": 15,
            "colSpan": 3,
            "rowSpan": 3
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "content": "### App Service Plan",
                "markdownSource": 1,
                "markdownUri": "",
                "subtitle": "",
                "title": ""
              }
            }
          }
        },
        "24": {
          "position": {
            "x": 3,
            "y": 15,
            "colSpan": 6,
            "rowSpan": 3
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
                    "title": "Avg CPU Percentage for ${plan_services_name}",
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
                        "hideSubtitle": false,
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
        "25": {
          "position": {
            "x": 9,
            "y": 15,
            "colSpan": 6,
            "rowSpan": 3
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
                      }
                    ],
                    "title": "Avg Memory Percentage for ${plan_services_name}",
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
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2
                      }
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
              "format": "local",
              "granularity": "auto",
              "relative": "24h"
            },
            "displayCache": {
              "name": "Local Time",
              "value": "Past 24 hours"
            },
            "filteredPartIds": [
              "StartboardPart-MonitorChartPart-c6c705b4-e6dc-480a-9706-9ce45ee558c1",
              "StartboardPart-MonitorChartPart-c6c705b4-e6dc-480a-9706-9ce45ee558c3",
              "StartboardPart-MonitorChartPart-c6c705b4-e6dc-480a-9706-9ce45ee558c5",
              "StartboardPart-MonitorChartPart-c6c705b4-e6dc-480a-9706-9ce45ee558c7",
              "StartboardPart-MonitorChartPart-c6c705b4-e6dc-480a-9706-9ce45ee558c9",
              "StartboardPart-MonitorChartPart-c6c705b4-e6dc-480a-9706-9ce45ee558cd",
              "StartboardPart-MonitorChartPart-c6c705b4-e6dc-480a-9706-9ce45ee558cf",
              "StartboardPart-MonitorChartPart-c6c705b4-e6dc-480a-9706-9ce45ee558d1",
              "StartboardPart-MonitorChartPart-c6c705b4-e6dc-480a-9706-9ce45ee558d5",
              "StartboardPart-MonitorChartPart-c6c705b4-e6dc-480a-9706-9ce45ee558d7",
              "StartboardPart-MonitorChartPart-c6c705b4-e6dc-480a-9706-9ce45ee558db",
              "StartboardPart-MonitorChartPart-c6c705b4-e6dc-480a-9706-9ce45ee558dd",
              "StartboardPart-LogsDashboardPart-c6c705b4-e6dc-480a-9706-9ce45ee558df",
              "StartboardPart-MonitorChartPart-c6c705b4-e6dc-480a-9706-9ce45ee558e7",
              "StartboardPart-MonitorChartPart-c6c705b4-e6dc-480a-9706-9ce45ee558e9",
              "StartboardPart-MonitorChartPart-c6c705b4-e6dc-480a-9706-9ce45ee558ed",
              "StartboardPart-MonitorChartPart-c6c705b4-e6dc-480a-9706-9ce45ee558ef",
              "StartboardPart-LogsDashboardPart-c6c705b4-e6dc-480a-9706-9ce45ee558f1"
            ]
          }
        }
      }
    }
  }
}