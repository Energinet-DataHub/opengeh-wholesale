{
  "lenses": [
    {
      "order": 0,
      "parts": [
        {
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
        {
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
                          "id": "${dropzoneunzipper_id}"
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
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Average memory working set"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${dropzoneunzipper_id}"
                        }
                      }
                    ],
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
        {
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
                          "id": "${dropzoneunzipper_id}"
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
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Function Execution Count"
                        },
                        "name": "FunctionExecutionCount",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${dropzoneunzipper_id}"
                        }
                      },
                      {
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Function Execution Units"
                        },
                        "name": "FunctionExecutionUnits",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${dropzoneunzipper_id}"
                        }
                      }
                    ],
                    "title": "Function Execution Count & Units",
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
        {
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
                          "id": "${dropzoneunzipper_id}"
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
                          "id": "${dropzoneunzipper_id}"
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
        {
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
                          "id": "${dropzoneunzipper_id}"
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
                    "title": "Sum Handle Count for ${dropzoneunzipper_name}",
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
                          "id": "${dropzoneunzipper_id}"
                        }
                      }
                    ],
                    "title": "Sum Handle Count for ${dropzoneunzipper_name}",
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
        {
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
                          "id": "${dropzoneunzipper_id}"
                        },
                        "name": "HealthCheckStatus",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Health check status",
                          "resourceDisplayName": "${dropzoneunzipper_name}"
                        }
                      }
                    ],
                    "title": "Avg Health check status for ${dropzoneunzipper_name}",
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
        {
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
        {
          "position": {
            "x": 3,
            "y": 3,
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
                          "displayName": "Response Time"
                        },
                        "name": "HttpResponseTime",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${dropzoneunzipper_id}"
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
                        "aggregationType": 7,
                        "metricVisualization": {
                          "displayName": "Response Time"
                        },
                        "name": "HttpResponseTime",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${dropzoneunzipper_id}"
                        }
                      }
                    ],
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
        {
          "position": {
            "x": 9,
            "y": 3,
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
                          "displayName": "Requests"
                        },
                        "name": "Requests",
                        "namespace": "microsoft.web/sites",
                        "resourceMetadata": {
                          "id": "${dropzoneunzipper_id}"
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
                          "id": "${dropzoneunzipper_id}"
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
        {
          "position": {
            "x": 15,
            "y": 3,
            "colSpan": 1,
            "rowSpan": 9
          },
          "metadata": {
            "inputs": [],
            "type": "Extension/HubsExtension/PartType/MarkdownPart",
            "settings": {
              "content": {
                "content": "# E\n# V\n# E\n# N\n# T\n# -\n# H\n# U\n# B\n",
                "markdownSource": 1,
                "markdownUri": "",
                "subtitle": "",
                "title": ""
              }
            }
          }
        },
        {
          "position": {
            "x": 16,
            "y": 3,
            "colSpan": 6,
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
                          "key": "EntityName",
                          "operator": 0,
                          "values": [
                            "${evh_dropzone_name}"
                          ]
                        }
                      ]
                    },
                    "metrics": [
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Incoming Messages",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "IncomingMessages",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Outgoing Messages",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "OutgoingMessages",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Captured Messages.",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "CapturedMessages",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 4,
                        "metricVisualization": {
                          "displayName": "Capture Backlog.",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "CaptureBacklog",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
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
                    "title": null,
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
                          "displayName": "Incoming Messages",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "IncomingMessages",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Outgoing Messages",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "OutgoingMessages",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Captured Messages.",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "CapturedMessages",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 4,
                        "metricVisualization": {
                          "displayName": "Capture Backlog.",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "CaptureBacklog",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      }
                    ],
                    "title": null,
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
              "EntityName": {
                "model": {
                  "operator": "equals",
                  "values": [
                    "${evh_dropzone_name}"
                  ]
                }
              }
            }
          }
        },
        {
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
        {
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
                          "id": "${dropzoneunzipper_id}"
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
                          "id": "${dropzoneunzipper_id}"
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
        {
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
                          "id": "${dropzoneunzipper_id}"
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
                          "id": "${dropzoneunzipper_id}"
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
                          "id": "${dropzoneunzipper_id}"
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
                          "id": "${dropzoneunzipper_id}"
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
                          "id": "${dropzoneunzipper_id}"
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
                          "id": "${dropzoneunzipper_id}"
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
                          "id": "${dropzoneunzipper_id}"
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
                          "id": "${dropzoneunzipper_id}"
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
                          "id": "${dropzoneunzipper_id}"
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
                          "id": "${dropzoneunzipper_id}"
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
                          "id": "${dropzoneunzipper_id}"
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
                          "id": "${dropzoneunzipper_id}"
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
        {
          "position": {
            "x": 16,
            "y": 6,
            "colSpan": 6,
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
                          "key": "EntityName",
                          "operator": 0,
                          "values": [
                            "${evh_dropzone_name}"
                          ]
                        }
                      ]
                    },
                    "metrics": [
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Incoming Requests",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "IncomingRequests",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Successful Requests",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "SuccessfulRequests",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Server Errors.",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "ServerErrors",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "User Errors.",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "UserErrors",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Throttled Requests.",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "ThrottledRequests",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
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
                    "title": null,
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
                          "displayName": "Incoming Requests",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "IncomingRequests",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Successful Requests",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "SuccessfulRequests",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Server Errors.",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "ServerErrors",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "User Errors.",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "UserErrors",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Throttled Requests.",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "ThrottledRequests",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      }
                    ],
                    "title": null,
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
              "EntityName": {
                "model": {
                  "operator": "equals",
                  "values": [
                    "${evh_dropzone_name}"
                  ]
                }
              }
            }
          }
        },
        {
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
        {
          "position": {
            "x": 3,
            "y": 9,
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
                          "id": "${dropzoneunzipper_id}"
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
                          "id": "${dropzoneunzipper_id}"
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
        {
          "position": {
            "x": 9,
            "y": 9,
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
                          "id": "${dropzoneunzipper_id}"
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
                          "id": "${dropzoneunzipper_id}"
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
        {
          "position": {
            "x": 16,
            "y": 9,
            "colSpan": 6,
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
                          "key": "EntityName",
                          "operator": 0,
                          "values": [
                            "${evh_dropzone_name}"
                          ]
                        }
                      ]
                    },
                    "metrics": [
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Incoming Bytes.",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "IncomingBytes",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Outgoing Bytes.",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "OutgoingBytes",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Captured Bytes.",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "CapturedBytes",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
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
                    "title": null,
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
                          "displayName": "Incoming Bytes.",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "IncomingBytes",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Outgoing Bytes.",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "OutgoingBytes",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      },
                      {
                        "aggregationType": 1,
                        "metricVisualization": {
                          "displayName": "Captured Bytes.",
                          "resourceDisplayName": "${evhns_dropzone_name}"
                        },
                        "name": "CapturedBytes",
                        "namespace": "microsoft.eventhub/namespaces",
                        "resourceMetadata": {
                          "id": "${evhns_dropzone_id}"
                        }
                      }
                    ],
                    "title": null,
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
              "EntityName": {
                "model": {
                  "operator": "equals",
                  "values": [
                    "${evh_dropzone_name}"
                  ]
                }
              }
            }
          }
        },
        {
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
                "content": "### Custom metrics",
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
            "x": 3,
            "y": 12,
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
                "value": "95c7f2e4-396a-4f7a-b12c-14daf04b6be2",
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
                "value": "traces\n| where cloud_RoleName in (\"${dropzoneunzipper_name}\")\n| where message startswith \"Trying to retrigger\"\n| extend \n    TsCount = toint(extract(\"Trying to retrigger (\\\\d+) files with TS prefix\", 1, message)),\n    MdCount = toint(extract(\"(\\\\d+) files with MD prefix\", 1, message)),\n    ChCount = toint(extract(\"(\\\\d+) files with CH prefix\", 1, message)),\n    ClCount = toint(extract(\"(\\\\d+) files with CL prefix\", 1, message)),\n    CsCount = toint(extract(\"(\\\\d+) files with CS prefix\", 1, message))\n| project timestamp, TsCount, MdCount, ChCount, ClCount, CsCount\n",
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
                "name": "Dimensions",
                "value": {
                  "xAxis": {
                    "name": "timestamp",
                    "type": "datetime"
                  },
                  "yAxis": [
                    {
                      "name": "TsCount",
                      "type": "int"
                    },
                    {
                      "name": "MdCount",
                      "type": "int"
                    },
                    {
                      "name": "ChCount",
                      "type": "int"
                    },
                    {
                      "name": "ClCount",
                      "type": "int"
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
                "PartTitle": "Reprocessed dead events"
              }
            }
          }
        },
        {
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
                "content": "### Failures",
                "markdownSource": 1,
                "markdownUri": "",
                "subtitle": "",
                "title": ""
              }
            }
          }
        },
        {
          "position": {
            "x": 3,
            "y": 15,
            "colSpan": 6,
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
                            "${dropzoneunzipper_name}"
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
                    "title": "Sum Exceptions for ${appi_sharedres_id} where Cloud role name = '${dropzoneunzipper_name}'",
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
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "exceptions/count",
                        "aggregationType": 1,
                        "namespace": "microsoft.insights/components/kusto",
                        "metricVisualization": {
                          "displayName": "Exceptions"
                        }
                      }
                    ],
                    "title": "Sum Exceptions for ${dropzoneunzipper_name}",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2,
                        "hideHoverCard": false,
                        "hideLabelNames": true
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
              "cloud/roleName": {
                "model": {
                  "operator": "equals",
                  "values": [
                    "${dropzoneunzipper_name}"
                  ]
                }
              }
            }
          }
        },
        {
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
                        "resourceMetadata": {
                          "id": "${dropzoneunzipper_id}"
                        },
                        "name": "InstanceCount",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Automatic Scaling Instance Count",
                          "resourceDisplayName": "${dropzoneunzipper_name}"
                        }
                      }
                    ],
                    "title": "Avg Automatic Scaling Instance Count for ${dropzoneunzipper_name}",
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
        {
          "position": {
            "x": 0,
            "y": 18,
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
        {
          "position": {
            "x": 3,
            "y": 18,
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
        {
          "position": {
            "x": 9,
            "y": 18,
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
              "format": "local",
              "granularity": "auto",
              "relative": "24h"
            },
            "displayCache": {
              "name": "Local Time",
              "value": "Past 24 hours"
            },
            "filteredPartIds": [
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e6b9",
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e6bb",
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e6bd",
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e6bf",
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e6c5",
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e6c7",
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e6cb",
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e6cf",
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e6d1",
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e6d3",
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e6d7",
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e6d9",
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e6db",
              "StartboardPart-LogsDashboardPart-b399bb4e-e921-4588-8dae-98ae552635d3",
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e6df",
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e7a0",
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e6e3",
              "StartboardPart-MonitorChartPart-774680db-542e-4418-af7d-58b57c69e6e5",
              "StartboardPart-MonitorChartPart-b399bb4e-e921-4588-8dae-98ae5526384a"
            ]
          }
        }
      }
    }
  }
}
