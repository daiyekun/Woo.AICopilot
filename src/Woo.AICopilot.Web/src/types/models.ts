import {type ChatChunk, type IntentResult, MessageRole, type Widget} from "@/types/protocols.ts";

// ---------------------- 前端数据结构 ----------------------

/**
 * 函数调用信息结构
 * FunctionCallContent + FunctionResultContent 合并
 */
export interface FunctionCall {
  id: string;
  name: string;
  args: string;
  result?: string;
  status: 'calling' | 'completed';
}

/**
 * 扩展消息块-意图识别块
 */
export interface IntentChunk extends ChatChunk {
  intents: IntentResult[]
}

/**
 * 扩展消息块-函数调用块
 */
export interface FunctionCallChunk extends ChatChunk {
  functionCall: FunctionCall;
}

/**
 * 扩展消息块-组件块
 */
export interface WidgetChunk extends ChatChunk {
  widget: Widget;
}

/**
 * 前端使用的消息模型
 */
export interface ChatMessage {
  sessionId: string;
  role: MessageRole;
  chunks: ChatChunk[];
  isStreaming: boolean;
  timestamp: number;
}
