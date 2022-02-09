import { Store, createStore, combineReducers } from 'redux';

interface TimerState {
    readonly working: 0;
    };

const initialTimerState: TimerState = {
    working: 1,
    };

export const GOT_TIMER_RESTART =
    'GOT_TIMER_RESTART';

export const gotTimerRestartAction = (
    isRestart
    ) =>
    ({
    type: GOT_TIMER_RESTART,
    working: isRestart,
    } as const);    

type TimerActions =
    | ReturnType<typeof gotTimerRestartAction>
    ;    

const timerReducer = (
        state = initialTimerState,
        action: TimerActions
        ) => {
            console.log('state.working=',state.working);
                switch (action.type) {
                    case GOT_TIMER_RESTART: {
                        return {
                            ...state, working: state.working + 1,
                        }
                    }   
                }    
        return state;
        };    



export function configureStore(): Store<TimerState> {
    const store = createStore(timerReducer);
    return store;
};
