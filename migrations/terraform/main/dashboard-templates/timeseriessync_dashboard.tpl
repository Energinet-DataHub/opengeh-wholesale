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
                  "title": "",
                  "subtitle": "",
                  "markdownSource": 1,
                  "markdownUri": null
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
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        },
                        "name": "AverageMemoryWorkingSet",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Average memory working set"
                        }
                      }
                    ],
                    "title": "Average memory usage over time",
                    "titleKind": 2,
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
                        "aggregationType": 1,
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
                        "isVisible": true,
                        "position": 2,
                        "hideSubtitle": false,
                        "hideHoverCard": false,
                        "hideLabelNames": false
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
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        },
                        "name": "CpuTime",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "CPU Time"
                        }
                      }
                    ],
                    "title": "Average memory usage over time",
                    "titleKind": 2,
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
                        "name": "FunctionExecutionCount",
                        "aggregationType": 1,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Count"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        },
                        "name": "FunctionExecutionUnits",
                        "aggregationType": 1,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Function Execution Units"
                        }
                      }
                    ],
                    "title": "Function Execution Count & Units",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideSubtitle": false,
                        "hideHoverCard": false,
                        "hideLabelNames": false
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
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        },
                        "name": "Threads",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Thread Count"
                        }
                      }
                    ],
                    "title": "Thread count",
                    "titleKind": 2,
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
                        "duration": 2592000000
                      },
                      "showUTCTime": false,
                      "grain": 1
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
                          "id": "${timeseriessync_id}"
                        },
                        "name": "Threads",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Thread Count"
                        }
                      }
                    ],
                    "title": "Thread count",
                    "titleKind": 2,
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
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        },
                        "name": "Handles",
                        "aggregationType": 1,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Handle Count"
                        }
                      }
                    ],
                    "title": "Sum Handle Count",
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
                        "aggregationType": 1,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Handle Count"
                        }
                      }
                    ],
                    "title": "Sum Handle Count",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideSubtitle": false,
                        "hideHoverCard": false,
                        "hideLabelNames": false
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
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        },
                        "name": "HealthCheckStatus",
                        "aggregationType": 4,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Health check status"
                        }
                      }
                    ],
                    "titleKind": 0,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "isVisible": true,
                        "position": 2,
                        "hideHoverCard": false,
                        "hideLabelNames": false
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
                        "hideLabelNames": false
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
                  "title": "",
                  "subtitle": "",
                  "markdownSource": 1,
                  "markdownUri": null
                }
              }
            }
          }
        },
        "7": {
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
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        },
                        "name": "HttpResponseTime",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Response Time"
                        }
                      }
                    ],
                    "title": "Response time",
                    "titleKind": 2,
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
                        "duration": 2592000000
                      },
                      "showUTCTime": false,
                      "grain": 1
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
                          "id": "${timeseriessync_id}"
                        },
                        "name": "HttpResponseTime",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Response Time"
                        }
                      }
                    ],
                    "title": "Response time",
                    "titleKind": 2,
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
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        },
                        "name": "Requests",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Requests"
                        }
                      }
                    ],
                    "title": "Total requests",
                    "titleKind": 2,
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
                        "duration": 2592000000
                      },
                      "showUTCTime": false,
                      "grain": 1
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
                          "id": "${timeseriessync_id}"
                        },
                        "name": "Requests",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Requests"
                        }
                      }
                    ],
                    "title": "Total requests",
                    "titleKind": 2,
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
        "9": {
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
                "content": "### Failures",
                "title": "",
                "subtitle": "",
                "markdownSource": 1,
                "markdownUri": {}
              }
            }
          }
        },
        "10": {
          "position": {
            "x": 3,
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
                          "key": "cloud/roleName",
                          "operator": 0,
                          "values": [
                            "${timeseriessync_name}"
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
                    "title": "Sum of Exceptions",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2,
                        "hideHoverCard": false,
                        "hideLabelNames": false
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
                    "${timeseriessync_name}"
                  ]
                }
              }
            }
          }
        },
        "11": {
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
                    "filterCollection": {
                      "filters": [
                        {
                          "key": "cloud/roleName",
                          "operator": 0,
                          "values": [
                            "${timeseriessync_name}"
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
                        "resourceMetadata": {
                          "id": "${appi_sharedres_id}"
                        },
                        "name": "traces/count",
                        "aggregationType": 1,
                        "namespace": "microsoft.insights/components/kusto",
                        "metricVisualization": {
                          "displayName": "Traces"
                        }
                      }
                    ],
                    "title": "Sum Traces with Error Severity",
                    "titleKind": 2,
                    "visualization": {
                      "chartType": 2,
                      "legendVisualization": {
                        "hideSubtitle": false,
                        "isVisible": true,
                        "position": 2,
                        "hideHoverCard": false,
                        "hideLabelNames": false
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
                    "${timeseriessync_name}"
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
        "12": {
          "position": {
            "x": 15,
            "y": 6,
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
                "value": "exceptions\n| where cloud_RoleName in (\"${timeseriessync_name}\")\n| summarize exceptionCount = count() by type\n| order by exceptionCount desc\n",
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
                "Query": "exceptions\n| where cloud_RoleName in (\"${timeseriessync_name}\")\n| summarize exceptionCount = count() by type\n| order by exceptionCount desc\n\n",
                "PartTitle": "Exception Overview"
              }
            }
          }
        },
        "13": {
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
                  "title": "",
                  "subtitle": "",
                  "markdownSource": 1,
                  "markdownUri": null
                }
              }
            }
          }
        },
        "14": {
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
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        },
                        "name": "BytesSent",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Data Out"
                        }
                      }
                    ],
                    "title": "Outgoing bandwidth",
                    "titleKind": 2,
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
                        "duration": 2592000000
                      },
                      "showUTCTime": false,
                      "grain": 1
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
                          "id": "${timeseriessync_id}"
                        },
                        "name": "BytesSent",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Data Out"
                        }
                      }
                    ],
                    "title": "Outgoing bandwidth",
                    "titleKind": 2,
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
        "15": {
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
                        "resourceMetadata": {
                          "id": "${timeseriessync_id}"
                        },
                        "name": "BytesReceived",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Data In"
                        }
                      }
                    ],
                    "title": "Incoming bandwidth",
                    "titleKind": 2,
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
                        "duration": 2592000000
                      },
                      "showUTCTime": false,
                      "grain": 1
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
                          "id": "${timeseriessync_id}"
                        },
                        "name": "BytesReceived",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Data In"
                        }
                      }
                    ],
                    "title": "Incoming bandwidth",
                    "titleKind": 2,
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
        "16": {
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
                "content": "### App Service Plan",
                "markdownSource": 1,
                "markdownUri": "",
                "subtitle": "",
                "title": ""
              }
            }
          }
        },
        "17": {
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
        "18": {
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
              "format": "utc",
              "granularity": "auto",
              "relative": "24h"
            },
            "displayCache": {
              "name": "UTC Time",
              "value": "Past 24 hours"
            },
            "filteredPartIds": [
              "StartboardPart-MonitorChartPart-61eddab9-d2ac-45e6-876b-03af9dc9fbd9",
              "StartboardPart-MonitorChartPart-61eddab9-d2ac-45e6-876b-03af9dc9fbdb",
              "StartboardPart-MonitorChartPart-61eddab9-d2ac-45e6-876b-03af9dc9fbdd",
              "StartboardPart-MonitorChartPart-61eddab9-d2ac-45e6-876b-03af9dc9fbdf",
              "StartboardPart-MonitorChartPart-61eddab9-d2ac-45e6-876b-03af9dc9fbe1",
              "StartboardPart-MonitorChartPart-61eddab9-d2ac-45e6-876b-03af9dc9fbe5",
              "StartboardPart-MonitorChartPart-61eddab9-d2ac-45e6-876b-03af9dc9fbe7",
              "StartboardPart-MonitorChartPart-61eddab9-d2ac-45e6-876b-03af9dc9fbeb",
              "StartboardPart-MonitorChartPart-61eddab9-d2ac-45e6-876b-03af9dc9fbed",
              "StartboardPart-LogsDashboardPart-61eddab9-d2ac-45e6-876b-03af9dc9fbef",
              "StartboardPart-MonitorChartPart-61eddab9-d2ac-45e6-876b-03af9dc9fbf3",
              "StartboardPart-MonitorChartPart-61eddab9-d2ac-45e6-876b-03af9dc9fbf5",
              "StartboardPart-MonitorChartPart-61eddab9-d2ac-45e6-876b-03af9dc9fbf9",
              "StartboardPart-MonitorChartPart-61eddab9-d2ac-45e6-876b-03af9dc9fbfb"
            ]
          }
        }
      }
    }
  }
}