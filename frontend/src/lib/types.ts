export interface Medicine {
    id: string;
    name: string;
    sku: string;
    activeIngredient: string;
    unit: string;
}

export interface MedicineBatch {
    id: string;
    batchNumber: string;
    expiryDate: string;
    quantity: number;
}

export interface Sale {
    id: string;
    receiptNumber: string;
    saleDate: string;
    totalAmount: number;
    processedByUserId: string;
}

export interface PagedMeta {
    pageNumber: number;
    pageSize: number;
    totalRecords: number;
    totalPages: number;
}