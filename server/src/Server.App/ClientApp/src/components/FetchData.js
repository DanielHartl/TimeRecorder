import React, { Component } from 'react';

export class FetchData extends Component {
  displayName = FetchData.name

  constructor(props) {
    super(props);
    this.state = { summary: [], loading: true, closed: false };
    
    FetchData.fetchData(this);
  }
  
  componentWillUnmount() {
    var state = this.state;
    state.closed = true;
    this.setState(state);
  }

  static fetchData(obj) {
    if(obj.state.closed) {
      return;
    }

    fetch('api/activitysummary?key=test')
      .then(response => response.json())
      .then(data => {
          obj.setState({ summary: data, loading: false });
          setTimeout(function() { FetchData.fetchData(obj) }, 1000);
      });
  }

  static renderSummaryTable(summary) {
    return (
      <table className='table'>
        <thead>
          <tr>
            <th>Start</th>
            <th>End</th>
            <th>Duration</th>
          </tr>
        </thead>
        <tbody>
        { summary.weekly.map(w => FetchData.renderWeeklySummary(w)) }
        </tbody>
      </table>
    );
  }

  static renderWeeklySummary(weekly) {
    return (
      <React.Fragment>
      <tr>
        <td colspan="2">Week of { weekly.weekstart }</td><td>{ weekly.total_duration }</td>
      </tr>
      { weekly.daily.map(d => FetchData.renderDailySummary(d)) }
      </React.Fragment>
    );
  }

  static renderDailySummary(daily) {
      return (
      <React.Fragment>
      <tr>
        <td colspan="2">{ daily.day }</td><td>{ daily.total_duration }</td>
      </tr>
      { daily.timeranges.map(t => FetchData.renderTimeRanges(t)) }
      </React.Fragment>
      );
  }

  static renderTimeRanges(t) {
      return (<tr>
        <td>{t.start}</td>
        <td>{t.end}</td>
        <td>{t.duration}</td>
      </tr>);
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : FetchData.renderSummaryTable(this.state.summary);

    return (
      <div>
        <h1>Activity summary</h1>
        <p>This component demonstrates fetching data from the server.</p>
        {contents}
      </div>
    );
  }
}
