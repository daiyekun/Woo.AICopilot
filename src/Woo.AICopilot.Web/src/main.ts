import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'

import App from './App.vue'

const app = createApp(App)  // 创建应用实例

app.use(createPinia())  // 注册状态管理插件
app.use(ElementPlus)    // 注册 ElementPlus 插件

app.mount('#app') // 挂载到 index.html 的 #app 节点
