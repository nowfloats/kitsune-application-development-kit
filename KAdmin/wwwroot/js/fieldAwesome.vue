<template>
        <input
        class="form-control"
        type="button"
        v-model="value" 
        :disabled="disabled"
        :maxlength="schema.max"
        :placeholder="schema.placeholder"
        :readonly="schema.readonly" >
</template>

<script>
        import {abstractField} from "VueFormGenerator";

   export default {
            mixins: [ abstractField ]
   };
</script>