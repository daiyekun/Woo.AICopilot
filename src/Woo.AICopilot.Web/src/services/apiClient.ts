import { token, baseUrl } from "@/appsetting";

/**
 * 基础 API 客户端
 * 封装了 fetch，统一处理 URL 前缀和 JSON 序列化
 */
export const apiClient = {
  /**
   * 获取统一的请求头
   */
  getHeaders(): HeadersInit {
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}` // 注入认证头
    };
  },

  /**
   * 发送 POST 请求
   */
  async post<T>(endpoint: string, body: any): Promise<T> {
    const response = await fetch(`${baseUrl}${endpoint}`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: JSON.stringify(body)
    });

    if (!response.ok) {
      // 如果 401，提示 Token 可能过期
      if (response.status === 401) {
        console.error("Token 无效或已过期");
      }
      throw new Error(`API Error: ${response.statusText}`);
    }

    // 尝试解析 JSON
    try {
      return await response.json();
    } catch {
      return {} as T; // 处理空响应
    }
  },

  /**
   * 发送 GET 请求
   */
  async get<T>(endpoint: string): Promise<T> {
    const response = await fetch(`${baseUrl}${endpoint}`, {
      method: 'GET',
      headers: this.getHeaders()
    });

    if (!response.ok) {
      if (response.status === 401) {
        console.error("Token 无效或已过期，请更新 TEST_TOKEN");
      }
      throw new Error(`API Error: ${response.statusText}`);
    }

    return await response.json();
  }
};
