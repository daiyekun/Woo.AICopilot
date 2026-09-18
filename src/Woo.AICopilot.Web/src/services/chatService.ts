import { fetchEventSource } from '@microsoft/fetch-event-source';
import { apiClient } from './apiClient';
import { token, baseUrl } from "@/appsetting";
import type {ChatChunk} from "@/types/protocols.ts";

/**
 * 定义流式回调函数的接口
 * 上层调用者（Store）通过这些回调接收数据
 */
interface StreamCallbacks {
  onChunkReceived: (chunk: ChatChunk) => void;       // 当收到数据块时
  onComplete: () => void;                            // 当流结束时
  onError: (err: any) => void;                       // 当发生错误时
}

export const chatService = {
  /**
   * 获取会话列表
   */
  async getSessions() {
    return await apiClient.get<any[]>('/aigateway/session/list');
  },

  /**
   * 创建新会话
   */
  async createSession() {
    return await apiClient.post<any>('/aigateway/session', { });
  },

  /**
   * 发送消息并接收流式响应
   * @param sessionId 会话ID
   * @param message 用户输入的内容
   * @param callbacks 回调函数集合
   */
  async sendMessageStream(sessionId: string, message: string, callbacks: StreamCallbacks) {
    const ctrl = new AbortController();

    try {
      // 使用微软的库发起 SSE 请求
      await fetchEventSource(`${baseUrl}/aigateway/chat`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          sessionId: sessionId,
          message: message
        }),
        signal: ctrl.signal,

        // 1. 处理连接打开
        async onopen(response) {
          if (response.ok) {
            return; // 连接成功
          } else {
            throw new Error(`连接失败: ${response.status}`);
          }
        },

        // 2. 处理消息接收
        onmessage(msg) {
          try {
            // 解析后端发来的 ChatChunk JSON
            const chunk: ChatChunk = JSON.parse(msg.data);
            console.log(chunk);
            callbacks.onChunkReceived(chunk);
          } catch (err) {
            console.error('无法解析区消息块:', err);
          }
        },

        // 3. 处理连接关闭
        onclose() {
          callbacks.onComplete();
        },

        // 保持连接，即使页面进入后台
        openWhenHidden: true
      });
    } catch (err) {
      callbacks.onError(err);
    }
  }
};
