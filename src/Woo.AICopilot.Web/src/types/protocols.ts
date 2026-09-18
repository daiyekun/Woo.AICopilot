// ---------------------- 数据传输对象 ----------------------
/**
 * 对应后端的 Session 实体
 * 简化的会话信息
 */
export interface Session {
  id: string;
  title: string;
}

/**
 * 消息发送者枚举
 */
export enum MessageRole {
  User = 'User',
  Assistant = 'Assistant'
}

/**
 * 对应后端的 ChunkType 枚举
 * 决定了消息流中的数据块是纯文本还是可视化组件
 */
export enum ChunkType {
  Error = 'Error',
  Text = 'Text',
  Intent = 'Intent',
  Widget = 'Widget',
  FunctionResult = 'FunctionResult',
  FunctionCall = 'FunctionCall'
}

/**
 * 对应后端的 ChatChunk 类
 * 这是流式响应中每一次传输的最小单元
 */
export interface ChatChunk {
  source: string;     // 执行器ID，用于追踪是谁生成的
  type: ChunkType;    // 数据类型
  content: string;    // 内容载体（文本或JSON字符串）
}

/**
 * 对应后端的 IntentResult 实体
 * 意图结果
 */
export interface IntentResult {
  intent: string;
  confidence: number;
  reasoning?: string;
  query?: string;
}

// ---------------------- 可视化组件相关定义 ----------------------

/**
 * 基础组件接口
 */
export interface Widget {
  id: string;      // 组件唯一标识
  type: string;    // 组件类型：'Chart', 'StatsCard', 'DataTable'
  title: string;  // 组件标题
  description: string; // 描述
  data: any;       // 具体的数据载体，根据类型不同而不同
}

/**
 * 对应后端的 ChartWidget
 */
export interface ChartWidget extends Widget {
  type: 'Chart';
  data: {
    category: 'Bar' | 'Line' | 'Pie';
    dataset: {
      dimensions: string[];
      source: Array<Record<string, any>>;
    };
    encoding: {
      x: string;
      y: string[];
      seriesName?: string;
    };
  };
}

/**
 * 对应后端的 StatsCardWidget
 */
export interface StatsCardWidget extends Widget {
  type: 'StatsCard';
  data: {
    label: string;
    value: string | number;
    unit?: string;
  };
}

/**
 * 对应后端的 DataTableWidget
 */
export interface DataTableWidget extends Widget {
  type: 'DataTable';
  data: {
    columns: Array<{
      key: string;
      label: string;
      dataType: 'string' | 'number' | 'date' | 'boolean';
    }>;
    rows: Array<Record<string, any>>;
  };
}
