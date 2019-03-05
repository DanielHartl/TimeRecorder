import React, { Component } from 'react';

Date.prototype.getWeekNumber = function(){
  var d = new Date(Date.UTC(this.getFullYear(), this.getMonth(), this.getDate()));
  var dayNum = d.getUTCDay() || 7;
  d.setUTCDate(d.getUTCDate() + 4 - dayNum);
  var yearStart = new Date(Date.UTC(d.getUTCFullYear(),0,1));
  return Math.ceil((((d - yearStart) / 86400000) + 1)/7)
};

const mapMap = function(map, mapFunction){
  const toReturn = [];
  map.forEach(function(value, key){
    toReturn.push(mapFunction(value, key));
  })
  return toReturn;
}

function groupBy(list, keyGetter) {
  const map = new Map();
  list.forEach((item) => {
      const key = keyGetter(item);
      const collection = map.get(key);
      if (!collection) {
          map.set(key, [item]);
      } else {
          collection.push(item);
      }
  });
  return map;
}

export class FetchData extends Component {
  displayName = FetchData.name

  constructor(props) {
    super(props);
    this.state = { summary: [], loading: true };
    
    var dt = new Date();
    var tz = dt.getTimezoneOffset();

    fetch('api/activitysummary?user=dh&timeZoneOffset=' + tz)
      .then(response => response.json())
      .then(data => {
        this.setState({ summary: data, loading: false });
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
        { mapMap(groupBy(summary.daily, x => new Date(x.day).getWeekNumber()), (d, k) => {
          return (
            <React.Fragment key={k}>
            <tr>
              <td colSpan="3"><b>Week { k }</b></td>
            </tr>
            { d.map(dx => FetchData.renderDailySummary(dx)) }
            </React.Fragment>
          );
          }) }
        </tbody>
      </table>
    );
  }

  static renderDailySummary(daily) {
    return (
      <React.Fragment key={daily.day}>
      <tr>
        <td colSpan="2">{ new Date(daily.day).toDateString() }</td><td>{ daily.total_duration }</td>
      </tr>
      { daily.timeranges.map(t => FetchData.renderTimeRanges(t)) }
      </React.Fragment>
      );
  }

  static renderTimeRanges(t) {
    return (<tr key={t.start}>
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
