-- =============================================================================
-- ERP Demo Database Initialization for AI Copilot Testing
-- 描述: 这是一个模拟电子产品销售的 ERP 数据库，设计用于测试 AI 的数据分析和 SQL 生成能力。
-- =============================================================================

DROP DATABASE IF EXISTS erp_demo;
CREATE DATABASE erp_demo;

\c erp_demo;

-- =============================================================================
-- 1. 商品主表 (base_products)
-- =============================================================================
CREATE TABLE base_products (
                               product_id SERIAL PRIMARY KEY,
                               product_name VARCHAR(200) NOT NULL,
                               sku_code VARCHAR(50) UNIQUE NOT NULL,
                               category VARCHAR(50),
                               brand VARCHAR(50),
                               std_price DECIMAL(10, 2), -- 标准零售价
                               cost_price DECIMAL(10, 2), -- 进货成本
                               status VARCHAR(20) DEFAULT 'Active',
                               created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE base_products IS '商品基础信息表，存储所有 SKU 的核心数据';
COMMENT ON COLUMN base_products.product_id IS '商品唯一标识 ID，用于关联库存和订单明细';
COMMENT ON COLUMN base_products.sku_code IS '库存单元编码 (Stock Keeping Unit)，业务侧常用的唯一商品编码';
COMMENT ON COLUMN base_products.category IS '商品分类，例如：Electronics(电子), Furniture(家具), Accessories(配件)';
COMMENT ON COLUMN base_products.std_price IS '标准建议零售价 (MSRP)，注意：实际订单成交价可能会低于此价格';
COMMENT ON COLUMN base_products.cost_price IS '商品进货成本价，用于计算毛利 (Profit = Revenue - Cost)';
COMMENT ON COLUMN base_products.status IS '商品状态: Active(在售), Discontinued(停产), OutOfStock(缺货)';

-- =============================================================================
-- 2. 客户表 (customers) - 用于客户画像分析
-- =============================================================================
CREATE TABLE customers (
                           customer_id SERIAL PRIMARY KEY,
                           customer_name VARCHAR(100) NOT NULL,
                           contact_email VARCHAR(100),
                           region VARCHAR(50),
                           industry VARCHAR(50),
                           level VARCHAR(20) DEFAULT 'Standard',
                           join_date DATE DEFAULT CURRENT_DATE
);

COMMENT ON TABLE customers IS '客户信息表，用于分析客户分布和购买行为';
COMMENT ON COLUMN customers.region IS '客户所属大区，如：North China(华北), East China(华东), South China(华南)';
COMMENT ON COLUMN customers.industry IS '客户所属行业，用于 B2B 分析，如：Technology(科技), Education(教育), Retail(零售)';
COMMENT ON COLUMN customers.level IS '客户等级: Standard(普通), VIP(重要客户), Partner(合作伙伴)';

-- =============================================================================
-- 3. 仓库库存表 (stock_inventory)
-- =============================================================================
CREATE TABLE stock_inventory (
                                 id SERIAL PRIMARY KEY,
                                 product_id INT NOT NULL,
                                 warehouse_name VARCHAR(50) NOT NULL,
                                 quantity INT DEFAULT 0,
                                 last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                 CONSTRAINT fk_inv_product FOREIGN KEY (product_id) REFERENCES base_products(product_id)
);

COMMENT ON TABLE stock_inventory IS '实时库存表，记录各仓库的商品持有量';
COMMENT ON COLUMN stock_inventory.warehouse_name IS '仓库名称，例如：Hangzhou_Main_WH(杭州主仓), Beijing_Branch_WH(北京分仓)';
COMMENT ON COLUMN stock_inventory.quantity IS '当前可用物理库存数量';

-- =============================================================================
-- 4. 销售订单主表 (orders) - 用于销售趋势分析
-- =============================================================================
CREATE TABLE orders (
                        order_id SERIAL PRIMARY KEY,
                        order_no VARCHAR(50) UNIQUE NOT NULL,
                        customer_id INT NOT NULL,
                        order_date TIMESTAMP NOT NULL,
                        total_amount DECIMAL(12, 2) DEFAULT 0,
                        status VARCHAR(20) DEFAULT 'Pending',
                        CONSTRAINT fk_order_customer FOREIGN KEY (customer_id) REFERENCES customers(customer_id)
);

COMMENT ON TABLE orders IS '销售订单主表，记录交易发生的时间、客户和总金额';
COMMENT ON COLUMN orders.order_no IS '业务订单号，格式通常为 ORD-YYYYMMDD-XXX';
COMMENT ON COLUMN orders.total_amount IS '订单总金额 (实际支付金额)，是该订单所有明细项 subtotal 的总和';
COMMENT ON COLUMN orders.status IS '订单状态: Pending(待付款), Paid(已付款), Shipped(已发货), Completed(已完成), Cancelled(已取消). 注意：统计销售业绩时通常应排除 Cancelled 状态';

-- =============================================================================
-- 5. 订单明细表 (order_items) - 用于商品销售分析
-- =============================================================================
CREATE TABLE order_items (
                             item_id SERIAL PRIMARY KEY,
                             order_id INT NOT NULL,
                             product_id INT NOT NULL,
                             quantity INT NOT NULL,
                             unit_price DECIMAL(10, 2) NOT NULL,
                             subtotal DECIMAL(12, 2) GENERATED ALWAYS AS (quantity * unit_price) STORED,
                             CONSTRAINT fk_item_order FOREIGN KEY (order_id) REFERENCES orders(order_id),
                             CONSTRAINT fk_item_product FOREIGN KEY (product_id) REFERENCES base_products(product_id)
);

COMMENT ON TABLE order_items IS '订单明细表，记录每一笔订单中包含的具体商品和数量';
COMMENT ON COLUMN order_items.unit_price IS '实际成交单价 (Actual Selling Price)，可能与商品表的标准售价不同（如有折扣）';
COMMENT ON COLUMN order_items.subtotal IS '行小计金额 = 数量 * 实际成交单价';

-- =============================================================================
-- 数据填充
-- =============================================================================

-- =============================================================================
-- 数据填充 (Mock Data Seed)
-- =============================================================================

-- 1. 商品数据 (Products)
-- 包含：电子产品、电脑、配件、家具等不同类目，用于测试分类统计
INSERT INTO base_products (product_name, sku_code, category, brand, std_price, cost_price) VALUES
                                                                                               ('iPhone 15 Pro', 'AP-IP15P-256', 'Electronics', 'Apple', 8999.00, 7500.00),
                                                                                               ('iPhone 15 Plus', 'AP-IP15PL-128', 'Electronics', 'Apple', 6999.00, 5800.00),
                                                                                               ('MacBook Pro M3', 'AP-MBP-M3-14', 'Electronics', 'Apple', 12999.00, 10500.00),
                                                                                               ('Dell XPS 15', 'DELL-XPS-9530', 'Computers', 'Dell', 11500.00, 9000.00),
                                                                                               ('Logitech MX Master 3S', 'LOGI-MX3S', 'Accessories', 'Logitech', 899.00, 500.00),
                                                                                               ('Sony WH-1000XM5', 'SNY-XM5-BLK', 'Audio', 'Sony', 2499.00, 1800.00),
                                                                                               ('ErgoChair Pro', 'FRN-CHR-PRO', 'Furniture', 'Autonomous', 3500.00, 2000.00);

-- 2. 客户数据 (Customers)
-- 包含：不同行业、不同等级（VIP/Standard）和不同地区的客户，用于画像分析
INSERT INTO customers (customer_name, region, industry, level, join_date) VALUES
                                                                              ('TechFlow Inc.', 'East China', 'Technology', 'VIP', '2023-01-10'),      -- 华东科技公司 (VIP)
                                                                              ('EduGlobal School', 'North China', 'Education', 'Partner', '2023-03-15'), -- 华北教育机构 (合作伙伴)
                                                                              ('Retail Solutions Ltd.', 'South China', 'Retail', 'Standard', '2023-05-20'), -- 华南零售商
                                                                              ('David Li', 'East China', 'Individual', 'Standard', '2023-06-01'),        -- 个人用户
                                                                              ('Green Energy Corp', 'West China', 'Energy', 'VIP', '2023-11-11');        -- 西部能源公司

-- 3. 库存数据 (Inventory)
-- 模拟：部分商品充足，部分商品缺货 (Stockout) 的场景
INSERT INTO stock_inventory (product_id, warehouse_name, quantity) VALUES
                                                                       (1, 'Hangzhou_Main_WH', 50), (1, 'Beijing_Branch_WH', 15), -- iPhone 15 Pro 库存充足
                                                                       (2, 'Hangzhou_Main_WH', 5),  (2, 'Beijing_Branch_WH', 0),  -- iPhone 15 Plus 北京仓缺货
                                                                       (3, 'Hangzhou_Main_WH', 20),                                -- MacBook 只有杭州仓有货
                                                                       (5, 'Hangzhou_Main_WH', 200), (5, 'Guangzhou_Branch_WH', 100); -- 鼠标在多地仓库都有大量库存

-- 4. 订单与明细数据 (Orders & Items)
-- 场景覆盖：企业采购、教育订单、取消订单、近期高额订单

-- 订单 1: 企业批量采购 (Apple 产品)
-- 测试点：大额订单、批量折扣逻辑（售价低于标价）
INSERT INTO orders (order_no, customer_id, order_date, status) VALUES ('ORD-20231001-001', 1, '2023-10-01 10:00:00', 'Completed');
INSERT INTO order_items (order_id, product_id, quantity, unit_price) VALUES
                                                                         (1, 1, 10, 8800.00), -- 购买10台 iPhone，单价 8800 (原价 8999)
                                                                         (1, 3, 5, 12500.00); -- 购买 5台 MacBook，单价 12500 (原价 12999)
UPDATE orders SET total_amount = (SELECT SUM(subtotal) FROM order_items WHERE order_id = 1) WHERE order_id = 1;

-- 订单 2: 教育机构采购 (Dell 电脑 + 配件)
-- 测试点：跨类目购买 (Computers + Accessories)、状态为"已发货"
INSERT INTO orders (order_no, customer_id, order_date, status) VALUES ('ORD-20231111-002', 2, '2023-11-11 14:30:00', 'Shipped');
INSERT INTO order_items (order_id, product_id, quantity, unit_price) VALUES
                                                                         (2, 4, 20, 11000.00), -- 20台 Dell XPS
                                                                         (2, 5, 20, 800.00);   -- 20个 罗技鼠标
UPDATE orders SET total_amount = (SELECT SUM(subtotal) FROM order_items WHERE order_id = 2) WHERE order_id = 2;

-- 订单 3: 个人用户购买 (已取消)
-- 测试点：在统计销售额时，应当排除状态为 Cancelled 的订单
INSERT INTO orders (order_no, customer_id, order_date, status) VALUES ('ORD-20231201-003', 4, '2023-12-01 09:15:00', 'Cancelled');
INSERT INTO order_items (order_id, product_id, quantity, unit_price) VALUES (3, 6, 1, 2499.00); -- 索尼耳机
UPDATE orders SET total_amount = (SELECT SUM(subtotal) FROM order_items WHERE order_id = 3) WHERE order_id = 3;

-- 订单 4: 近期高价值订单
-- 测试点：时间范围查询 (例如查询"最近一个月")、高客单价商品
INSERT INTO orders (order_no, customer_id, order_date, status) VALUES ('ORD-20231220-004', 5, '2023-12-20 16:00:00', 'Paid');
INSERT INTO order_items (order_id, product_id, quantity, unit_price) VALUES
    (4, 7, 10, 3200.00); -- 10把人体工学椅
UPDATE orders SET total_amount = (SELECT SUM(subtotal) FROM order_items WHERE order_id = 4) WHERE order_id = 4;