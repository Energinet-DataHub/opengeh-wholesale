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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "CpuTime",
                        "aggregationType": 7,
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
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
            },
            "filters": {
              "MsPortalFx_TimeRange": {
                "model": {
                  "format": "local",
                  "granularity": "auto",
                  "relative": "43200m"
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "Handles",
                        "aggregationType": 1,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Handle Count"
                        }
                      }
                    ],
                    "title": "Sum Handle Count for ${timeseriesapi_subscription_id}",
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "Handles",
                        "aggregationType": 1,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Handle Count"
                        }
                      }
                    ],
                    "title": "Sum Handle Count for ${timeseriesapi_subscription_id}",
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
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
            },
            "filters": {
              "MsPortalFx_TimeRange": {
                "model": {
                  "format": "local",
                  "granularity": "auto",
                  "relative": "1440m"
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
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
            },
            "filters": {
              "MsPortalFx_TimeRange": {
                "model": {
                  "format": "local",
                  "granularity": "auto",
                  "relative": "43200m"
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
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
            },
            "filters": {
              "MsPortalFx_TimeRange": {
                "model": {
                  "format": "local",
                  "granularity": "auto",
                  "relative": "43200m"
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
                "settings": {
                  "content": "### Http codes",
                  "title": "",
                  "subtitle": "",
                  "markdownSource": 1,
                  "markdownUri": null
                }
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
                "name": "options",
                "value": {
                  "chart": {
                    "metrics": [
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "Http5xx",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Http Server Errors"
                        }
                      }
                    ],
                    "title": "Count of Http server errors",
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "Http5xx",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Http Server Errors"
                        }
                      }
                    ],
                    "title": "Count of Http server errors",
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
            },
            "filters": {
              "MsPortalFx_TimeRange": {
                "model": {
                  "format": "local",
                  "granularity": "auto",
                  "relative": "43200m"
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
                    "metrics": [
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "Http401",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Http 401"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "Http403",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Http 403"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "Http404",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Http 404"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "Http404",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Http 404"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "Http406",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Http 406"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "Http2xx",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Http 2xx"
                        }
                      }
                    ],
                    "title": "Count of Http status codes",
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "Http401",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Http 401"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "Http403",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Http 403"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "Http404",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Http 404"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "Http404",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Http 404"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "Http406",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Http 406"
                        }
                      },
                      {
                        "resourceMetadata": {
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
                        },
                        "name": "Http2xx",
                        "aggregationType": 7,
                        "namespace": "microsoft.web/sites",
                        "metricVisualization": {
                          "displayName": "Http 2xx"
                        }
                      }
                    ],
                    "title": "Count of Http status codes",
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
        "12": {
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
        "13": {
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
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
        "14": {
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
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
                          "id": "/subscriptions/${timeseriesapi_subscription_id}/resourceGroups/${timeseriesapi_resource_group_name}/providers/Microsoft.Web/sites/${timeseriesapi_name}"
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
              "StartboardPart-MonitorChartPart-7d899c0b-ee13-467f-9f6c-2364080b60e1",
              "StartboardPart-MonitorChartPart-7d899c0b-ee13-467f-9f6c-2364080b60e3",
              "StartboardPart-MonitorChartPart-7d899c0b-ee13-467f-9f6c-2364080b60e5",
              "StartboardPart-MonitorChartPart-7d899c0b-ee13-467f-9f6c-2364080b60e7",
              "StartboardPart-MonitorChartPart-7d899c0b-ee13-467f-9f6c-2364080b60eb",
              "StartboardPart-MonitorChartPart-7d899c0b-ee13-467f-9f6c-2364080b60ed",
              "StartboardPart-MonitorChartPart-7d899c0b-ee13-467f-9f6c-2364080b60f1",
              "StartboardPart-MonitorChartPart-7d899c0b-ee13-467f-9f6c-2364080b60f3",
              "StartboardPart-MonitorChartPart-7d899c0b-ee13-467f-9f6c-2364080b60f7",
              "StartboardPart-MonitorChartPart-7d899c0b-ee13-467f-9f6c-2364080b60f9"
            ]
          }
        }
      }
    }
  }
}