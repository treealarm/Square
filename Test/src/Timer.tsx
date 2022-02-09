import React, {

  } from "react";
import { connect } from "react-redux";


  const mapStateToProps = (state, ownProps) => ({ 
    working: state.working,
    // any props you need else
 });

export class TimerVal extends React.Component {
    constructor(props) {
      super(props);
      
      this.state = {
        timer_val : 11
      }
      this.handleChange = this.handleChange.bind(this);    
      this.startTimer = this.startTimer.bind(this);
      this.countDown = this.countDown.bind(this);
      }
   
    handleChange(e) {
      this.props.onTimerTick(e);
    }

    componentDidUpdate(prevProps, prevState, snapshot) {
      if (prevProps.working !== this.props.working) {
          console.log("reset timer");
          this.state.timer_val = 10;
      }
    }
  
    incrementCount() {
      
      this.setState((state) => {
        return {timer_val: state.timer_val + 1}
      });

            
      //this.handleChange(this.state.timer_val);
    }
  
    startTimer = () => {
  
      if(this.timer != null)
      {
        clearInterval(this.timer);
      }
      this.timer = setInterval(this.countDown, 1000);
      
      console.log('from timer:start timer')
    }
  
    countDown = () => {
      this.incrementCount();
    }
  
    componentDidMount() {
        
      this.startTimer();
    }
  
    componentWillUnmount() {
      clearInterval(this.timer);      
    }
  
    render() {
  
      const { timer_val } = this.state;
      return (
        <div>
        <h1>{timer_val}</h1>
      </div>
      );
    }
  }

  TimerVal = connect(mapStateToProps)(TimerVal);