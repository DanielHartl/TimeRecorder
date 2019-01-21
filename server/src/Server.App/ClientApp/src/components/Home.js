import React, { Component } from 'react';
import CalendarHeatmap from 'reactjs-calendar-heatmap'

var data = [{
  "date": "2019-01-01",
  "total": 17164,
  "details": [{
    "name": "Project 1",
    "date": "2019-01-01 12:30:45",
    "value": 9192
  },
  {
    "name": "Project 1",
    "date": "2019-01-01 13:37:00",
    "value": 6753
  },
  {
    "name": "Project 1",
    "date": "2019-01-01 17:52:41",
    "value": 2219
  }]
  },
  {
    "date": "2019-01-02",
    "total": 35,
    "details": [{
      "name": "Project 1",
      "date": "2019-01-02 12:35:45",
      "value": 12
    }]
}]

export class Home extends Component {
  displayName = Home.name

  render() {
    return (
      <div>
        <h1>Today's overview</h1>
        <CalendarHeatmap
          data={data}
          overview='month'>    
        </CalendarHeatmap>
      </div>
    );
  }
}
