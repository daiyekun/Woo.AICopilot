<script setup lang="ts">
import { computed } from 'vue';
import { useChatStore } from '@/stores/chatStore.ts';
import { Plus, ChatDotRound } from '@element-plus/icons-vue';

// 连接 Store
const store = useChatStore();

// 计算属性：会话列表
const sessions = computed(() => store.sessions);
const currentId = computed(() => store.currentSessionId);

// 处理点击
const handleSelect = (id: string) => {
  store.selectSession(id);
};

const handleNewChat = () => {
  store.createNewSession();
};
</script>

<template>
  <div class="session-sidebar">
    <div class="sidebar-header">
      <el-button
        type="primary"
        class="new-chat-btn"
        :icon="Plus"
        @click="handleNewChat"
      >
        新建会话
      </el-button>
    </div>

    <div class="session-list">
      <div
        v-for="session in sessions"
        :key="session.id"
        class="session-item"
        :class="{ active: currentId === session.id }"
        @click="handleSelect(session.id)"
      >
        <el-icon class="icon"><ChatDotRound /></el-icon>
        <span class="title">{{ session.title }}</span>
      </div>

      <div v-if="sessions.length === 0" class="empty-tip">
        暂无历史会话
      </div>
    </div>
  </div>
</template>

<style scoped>
.session-sidebar {
  display: flex;
  flex-direction: column;
  height: 100%;
  background-color: var(--bg-color-secondary);
  border-right: 1px solid var(--border-color);
}

.sidebar-header {
  padding: 20px;
  flex-shrink: 0; /* 防止头部被压缩 */
}

.new-chat-btn {
  width: 100%;
  border-radius: 8px;
}

.session-list {
  flex: 1; /* 占据剩余高度 */
  overflow-y: auto; /* 内部滚动 */
  padding: 0 12px;
}

.session-item {
  display: flex;
  align-items: center;
  padding: 12px 16px;
  margin-bottom: 8px;
  border-radius: 8px;
  cursor: pointer;
  color: var(--text-primary);
  transition: all 0.2s ease;
  font-size: 14px;
}

.session-item:hover {
  background-color: rgba(0, 0, 0, 0.05);
}

.session-item.active {
  background-color: #e6f0ff; /* 激活态背景色 */
  color: var(--brand-color);
  font-weight: 500;
}

.session-item .icon {
  margin-right: 10px;
  font-size: 16px;
}

.session-item .title {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis; /* 文字过长显示省略号 */
}

.empty-tip {
  text-align: center;
  color: var(--text-secondary);
  font-size: 12px;
  margin-top: 20px;
}
</style>
