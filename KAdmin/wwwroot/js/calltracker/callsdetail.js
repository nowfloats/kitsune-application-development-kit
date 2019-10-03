var CallDetails = Vue.component("calls-detail", {
    template: "#callsdetail",
    name: "CallsDetail",

    props: {
        "missedCallsCount": {
            type: Number,
            required: true
        },
        "connectedCallsCount": {
            type: Number,
            required: true
        },
        "totalCallsCount": {
            type: Number,
            required: true
        }
    }
});