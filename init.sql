-- UUID生成拡張機能
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 在庫テーブル（今回は簡略化して商品マスタ兼在庫）
CREATE TABLE items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    stock INT NOT NULL CHECK (stock >= 0), -- DBレベルでマイナス在庫を許さない（堅牢性）
    version INT DEFAULT 1 -- 楽観ロック用
);

-- 注文テーブル
CREATE TABLE orders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    item_id UUID NOT NULL REFERENCES items(id),
    user_id UUID NOT NULL,
    quantity INT NOT NULL,
    created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- テストデータ投入 (IDは固定しておくとテストしやすい)
INSERT INTO items (id, name, price, stock) 
VALUES ('11111111-1111-1111-1111-111111111111', 'Platinum Ticket', 10000, 100);