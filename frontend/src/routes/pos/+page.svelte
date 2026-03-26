<script lang="ts">
    import { onMount } from 'svelte';
    import { api } from '$lib/api';
    import type { Medicine, MedicineCheckout } from '$lib/types';

    let medicines: Medicine[] = $state([]);
    let cart: (MedicineCheckout & { name: string })[] = $state([]);
    let selectedMedicineId = $state('');
    let selectedQuantity = $state(1);
    let isLoading = $state(true);
    let errorMessage = $state('');
    let isCheckingOut = $state(false);
    
    async function fetchMedicines() {
        try {
            const response = await api.get('/medicines');
            const payload = response.data;
            const list = payload?.data ?? payload?.Data ?? payload;

            medicines = Array.isArray(list) ? list : [];
            errorMessage = '';
        } catch (error: any) {
            errorMessage = error.response?.data?.message || 'Network error occurred.';
            console.error("API Error:", error);
        } finally {
            isLoading = false;
        }
    }

    function addToCart() {
        if (!selectedMedicineId || selectedQuantity <= 0) {
            alert('Please select a medicine and quantity');
            return;
        }

        const medicine = medicines.find(m => m.id === selectedMedicineId);
        if (!medicine) return;

        // Check if medicine already in cart
        const existingItem = cart.find(item => item.medicineId === selectedMedicineId);
        if (existingItem) {
            existingItem.quantity += selectedQuantity;
        } else {
            cart.push({
                medicineId: selectedMedicineId,
                quantity: selectedQuantity,
                name: medicine.name
            });
        }

        // Reset form
        selectedMedicineId = '';
        selectedQuantity = 1;
        cart = cart; // Trigger reactivity
    }

    function removeFromCart(medicineId: string) {
        cart = cart.filter(item => item.medicineId !== medicineId);
    }

    function updateQuantity(medicineId: string, newQuantity: number) {
        const item = cart.find(item => item.medicineId === medicineId);
        if (item) {
            if (newQuantity <= 0) {
                removeFromCart(medicineId);
            } else {
                item.quantity = newQuantity;
                cart = cart; // Trigger reactivity
            }
        }
    }

    async function checkoutCart() {
        if (cart.length === 0) {
            alert('Cart is empty');
            return;
        }

        isCheckingOut = true;
        try {
            await api.post(`medicines/checkout`, { items: cart });
            alert('Checkout successful!');
            cart = [];
            fetchMedicines();
        } catch (error: any) {
            alert(error.response?.data?.message || 'Failed to checkout medicines.');
            console.error("Checkout Error:", error);
        } finally {
            isCheckingOut = false;
        }
    }

    onMount(fetchMedicines);
</script>

<main class="min-h-screen bg-gray-900 text-white p-8">
    <div class="max-w-7xl mx-auto">
        <h1 class="text-3xl font-bold mb-8">Point of Sale</h1>
        
        {#if isLoading}
            <div class="flex justify-center items-center py-20 text-gray-400">
                <span class="animate-pulse">Loading inventory...</span>
            </div>
        {:else if errorMessage}
            <div class="bg-red-500/10 border border-red-500 text-red-400 p-4 rounded mb-8">
                {errorMessage}
            </div>
        {:else}
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
                <!-- Medicines List -->
                <div class="lg:col-span-2">
                    <h2 class="text-xl font-semibold mb-4">Select Medicines</h2>
                    {#if medicines.length == 0}
                        <div class="text-center py-20 text-gray-400 bg-gray-800 rounded-lg border border-gray-700">
                            No medicines found. Add some stock to get started.
                        </div>
                    {:else}
                        <div class="bg-gray-800 rounded-lg shadow overflow-hidden border border-gray-700">
                            <table class="min-w-full divide-y divide-gray-700">
                                <thead class="bg-gray-900/50">
                                    <tr>
                                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Name</th>
                                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">SKU</th>
                                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Ingredient</th>
                                        <th class="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">Unit</th>
                                        <th class="px-6 py-3 text-right text-xs font-medium text-gray-400 uppercase tracking-wider">Action</th>
                                    </tr>
                                </thead>
                                <tbody class="divide-y divide-gray-700 bg-gray-800">
                                    {#each medicines as medicine}
                                        <tr class="hover:bg-gray-750 transition">
                                            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-white">
                                                {medicine.name}
                                            </td>
                                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
                                                {medicine.sku}
                                            </td>
                                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
                                                {medicine.activeIngredient}
                                            </td>
                                            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
                                                {medicine.unit}
                                            </td>
                                            <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                                <button 
                                                    onclick={() => {
                                                        selectedMedicineId = medicine.id;
                                                        selectedQuantity = 1;
                                                    }}
                                                    class="text-blue-400 hover:text-blue-300 transition"
                                                >
                                                    Add
                                                </button>
                                            </td>
                                        </tr>
                                    {/each}
                                </tbody>
                            </table>
                        </div>
                    {/if}
                </div>

                <!-- Cart Sidebar -->
                <div class="lg:col-span-1">
                    <div class="bg-gray-800 rounded-lg border border-gray-700 p-6 sticky top-8">
                        <h2 class="text-xl font-semibold mb-6">Shopping Cart</h2>

                        <!-- Add to Cart Form -->
                        {#if medicines.length > 0}
                            <div class="mb-6 pb-6 border-b border-gray-700">
                                <label for="medicine-select" class="block text-sm font-medium text-gray-300 mb-2">
                                    Medicine
                                </label>
                                <select 
                                    id="medicine-select"
                                    bind:value={selectedMedicineId}
                                    class="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded text-white text-sm mb-3"
                                >
                                    <option value="">Select a medicine...</option>
                                    {#each medicines as medicine}
                                        <option value={medicine.id}>{medicine.name}</option>
                                    {/each}
                                </select>

                                <label for="quantity-input" class="block text-sm font-medium text-gray-300 mb-2">
                                    Quantity
                                </label>
                                <input 
                                    id="quantity-input"
                                    type="number" 
                                    min="1"
                                    bind:value={selectedQuantity}
                                    class="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded text-white text-sm mb-3"
                                />

                                <button 
                                    onclick={addToCart}
                                    class="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 rounded transition"
                                >
                                    Add to Cart
                                </button>
                            </div>
                        {/if}

                        <!-- Cart Items -->
                        {#if cart.length === 0}
                            <div class="text-center py-8 text-gray-400">
                                Cart is empty
                            </div>
                        {:else}
                            <div class="space-y-3 mb-6 max-h-96 overflow-y-auto">
                                {#each cart as item}
                                    <div class="bg-gray-700 rounded p-3">
                                        <div class="flex justify-between items-start mb-2">
                                            <span class="font-medium text-sm text-white">{item.name}</span>
                                            <button 
                                                onclick={() => removeFromCart(item.medicineId)}
                                                class="text-red-400 hover:text-red-300 text-xs"
                                            >
                                                Remove
                                            </button>
                                        </div>
                                        <div class="flex items-center gap-2">
                                            <button 
                                                onclick={() => updateQuantity(item.medicineId, item.quantity - 1)}
                                                class="px-2 py-1 bg-gray-600 hover:bg-gray-500 rounded text-xs"
                                            >
                                                -
                                            </button>
                                            <span class="flex-1 text-center text-sm">{item.quantity}</span>
                                            <button 
                                                onclick={() => updateQuantity(item.medicineId, item.quantity + 1)}
                                                class="px-2 py-1 bg-gray-600 hover:bg-gray-500 rounded text-xs"
                                            >
                                                +
                                            </button>
                                        </div>
                                    </div>
                                {/each}
                            </div>

                            <button 
                                onclick={checkoutCart}
                                disabled={isCheckingOut}
                                class="w-full bg-emerald-600 hover:bg-emerald-700 disabled:bg-gray-600 text-white font-medium py-2 rounded transition"
                            >
                                {isCheckingOut ? 'Processing...' : `Checkout (${cart.length} items)`}
                            </button>
                        {/if}
                    </div>
                </div>
            </div>
        {/if}
    </div>
</main>