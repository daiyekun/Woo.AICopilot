import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import { env } from 'node:process';
import { baseUrl } from './src/appsetting'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig(({ mode })=> {
  // 动态获取 Aspire 注入的后端地址
  // 格式通常为: {服务名}_http
  // 这里的 'aicopilot_httpapi' 必须与 AppHost 中 AddProject 的名称一致
  const target = env.aicopilot_httpapi_http;

  console.log('Detected Backend Target:', target); // 调试用，启动时打印地址

  return {
    plugins: [
      vue(),
      vueDevTools(),
    ],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url))
      },
    },
    server: {
      host: true,
      proxy: {
        // 配置反向代理
        // 所有以 baseUrl 开头的请求，都会被转发到 target
        [baseUrl]: {
          target: target,
          changeOrigin: true,
          secure: false, // 开发环境通常使用自签名证书，需要关闭安全验证
        }
      }
    }
  }
})
