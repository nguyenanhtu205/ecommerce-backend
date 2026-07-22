-- Cần cho emailDelivery.worker.ts để giới hạn số lần retry (tránh vòng lặp
-- vô hạn với 1 delivery lỗi vĩnh viễn, vd sai template_code).
ALTER TABLE message_deliveries
    ADD COLUMN attempts int NOT NULL DEFAULT 0;