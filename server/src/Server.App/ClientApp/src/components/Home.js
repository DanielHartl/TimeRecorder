import React, { Component } from 'react';
import CalendarHeatmap from 'react-calendar-heatmap';
import 'react-calendar-heatmap/dist/styles.css';

export class Home extends Component {
  displayName = Home.name

  constructor(props) {
    super(props);
    this.state = { loading: true, summary: [] };
    
    var dt = new Date();
    var tz = dt.getTimezoneOffset();

    fetch('api/activitysummary?user=dh&timeZoneOffset=' + tz)
      .then(response => response.json())
      .then(data => {
        this.setState({ summary: data, loading: false });
      });
  }

  static toCssClass(durationInMinutes) {
    var value = Math.floor(durationInMinutes / 120);
    if(value < 0) value = 0;
    if(value > 5) value = 5;
    return value;
  }

  static renderCalendar(summary) {
    let lastDay = summary.daily[0];
    let firstDay = summary.daily[summary.daily.length - 1];

    let values = summary.daily.map(d => ({ date: new Date(d.day), duration: Home.toCssClass(d.total_duration_in_minutes) }) );
    console.log(values);

    return (<CalendarHeatmap
      values={values}
      classForValue={(value) => {
        if (!value) {
          return 'color-empty';
        }
        return `color-scale-${value.duration}`;
      }}
      />)
  }

  render() {
    let contents = this.state.loading
    ? <p><em>Loading...</em></p>
    : <React.Fragment>
      <h1>Today's overview</h1>
      { Home.renderCalendar(this.state.summary) }
      </React.Fragment>

    return (
      <div>
        {contents}
      </div>
    );
  }
}
