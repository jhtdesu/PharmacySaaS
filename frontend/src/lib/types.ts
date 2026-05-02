export interface Medicine {
    id: string;
    name: string;
    sku: string;
    activeIngredient: string;
    unit: string;
    imageUrl: string;
}

export interface MedicineCheckout {
    medicineId: string;
    quantity: number;
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

export interface Tenant {
    id: string;
    storeName: string;
    address: string;
    phoneNumber: string;
    subscription: 'Free' | 'Basic' | 'Premium';
    subscriptionExpiry: string;
    subscriptionStatus: 'Active' | 'Trialing' | 'Canceled';
    isActive: boolean;
    createdAt: string;
}