<script lang="ts">
	import { onMount } from 'svelte';
	import { authApi, api } from '$lib/api';
	import type { Tenant } from '$lib/types';

	let tenant: Tenant | null = $state(null);
	let isLoading = $state(true);
	let isProcessing = $state(false);
	let errorMessage = $state('');
	let successMessage = $state('');
	let tenantId = $state('');

	onMount(async () => {
		const storedTenantId = localStorage.getItem('tenant_id');
		if (!storedTenantId) {
			errorMessage = 'Tenant ID not found. Please login again.';
			isLoading = false;
			return;
		}

		tenantId = storedTenantId;
		await fetchTenantInfo();
	});

	async function fetchTenantInfo() {
		isLoading = true;
		errorMessage = '';
		try {
			const response = await authApi.get(`/tenant/${tenantId}`);
			const tenantData = response.data?.data || response.data;
			tenant = tenantData;
		} catch (error: any) {
			errorMessage = error.response?.data?.message || 'Failed to load tenant information.';
			console.error('API Error:', error);
		} finally {
			isLoading = false;
		}
	}

	async function buySubscription() {
		isProcessing = true;
		errorMessage = '';
		successMessage = '';

		try {
			const userName = localStorage.getItem('user_name') || 'Customer';

			const response = await authApi.post('/tenant/buy-subscription', {
				tenantId,
				fullName: userName
			});

			const paymentLink = response.data?.data?.payUrl || response.data?.payUrl;

			if (paymentLink) {
				window.location.href = paymentLink;
			} else {
				errorMessage = 'Failed to generate payment link.';
			}
		} catch (error: any) {
			errorMessage = error.response?.data?.message || 'Failed to process subscription purchase.';
			console.error('API Error:', error);
		} finally {
			isProcessing = false;
		}
	}

	function formatDate(dateValue: string | Date) {
		if (!dateValue) return 'N/A';
		const date = new Date(dateValue);
		return date.toLocaleDateString('en-US', {
			year: 'numeric',
			month: 'short',
			day: 'numeric'
		});
	}

	function getSubscriptionBadgeColor(subscription: string) {
		switch (subscription) {
			case 'Premium':
				return 'bg-yellow-600 text-yellow-100';
			case 'Basic':
				return 'bg-blue-600 text-blue-100';
			default:
				return 'bg-gray-600 text-gray-100';
		}
	}

	function getStatusBadgeColor(status: string) {
		switch (status) {
			case 'Active':
				return 'bg-green-600 text-green-100';
			case 'Trialing':
				return 'bg-blue-600 text-blue-100';
			case 'Canceled':
				return 'bg-red-600 text-red-100';
			default:
				return 'bg-gray-600 text-gray-100';
		}
	}

	function isSubscriptionExpired() {
		if (!tenant?.subscriptionExpiry) return false;
		return new Date(tenant.subscriptionExpiry) < new Date();
	}

	function getDaysUntilExpiry() {
		if (!tenant?.subscriptionExpiry) return 0;
		const now = new Date();
		const expiry = new Date(tenant.subscriptionExpiry);
		const diffTime = expiry.getTime() - now.getTime();
		const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
		return Math.max(0, diffDays);
	}
</script>

<div class="min-h-screen bg-gray-900 text-white p-6">
	<div class="max-w-4xl mx-auto">
		<h1 class="text-4xl font-bold mb-8">Subscription Management</h1>

		{#if isLoading}
			<div class="text-center py-12">
				<p class="text-gray-400">Loading subscription information...</p>
			</div>
		{:else if errorMessage}
			<div class="bg-red-900 border border-red-700 rounded-lg p-4 mb-6">
				<p class="text-red-200">{errorMessage}</p>
			</div>
		{/if}

		{#if tenant}
			<div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
				<!-- Current Subscription Info -->
				<div class="bg-gray-800 rounded-lg border border-gray-700 p-6">
					<h2 class="text-2xl font-bold mb-4">Current Subscription</h2>

					<div class="space-y-4">
						<div>
							<p class="text-gray-400 text-sm mb-1">Store Name</p>
							<p class="text-lg font-semibold">{tenant.storeName}</p>
						</div>

						<div>
							<p class="text-gray-400 text-sm mb-1">Subscription Plan</p>
							<span class={`inline-block px-3 py-1 rounded-full text-sm font-semibold ${getSubscriptionBadgeColor(tenant.subscription)}`}>
								{tenant.subscription}
							</span>
						</div>

						<div>
							<p class="text-gray-400 text-sm mb-1">Status</p>
							<span class={`inline-block px-3 py-1 rounded-full text-sm font-semibold ${getStatusBadgeColor(tenant.subscriptionStatus)}`}>
								{tenant.subscriptionStatus}
							</span>
						</div>

						<div>
							<p class="text-gray-400 text-sm mb-1">Expiry Date</p>
							<p class="text-lg font-semibold">
								{formatDate(tenant.subscriptionExpiry)}
							</p>
							{#if isSubscriptionExpired()}
								<p class="text-red-400 text-sm mt-1">⚠️ Subscription expired</p>
							{:else}
								<p class="text-green-400 text-sm mt-1">✓ {getDaysUntilExpiry()} days remaining</p>
							{/if}
						</div>
					</div>
				</div>

				<!-- Subscription Plans -->
				<div class="bg-gray-800 rounded-lg border border-gray-700 p-6">
					<h2 class="text-2xl font-bold mb-4">Available Plans</h2>

					<div class="space-y-4">
						<div class="bg-gray-700 rounded-lg p-4 border border-yellow-600">
							<div class="flex justify-between items-start mb-2">
								<h3 class="text-xl font-bold text-yellow-400">Premium Plan</h3>
								<span class="text-2xl font-bold">500,000₫</span>
							</div>
							<p class="text-gray-300 text-sm mb-3">Per month</p>
							<ul class="text-gray-300 text-sm space-y-2 mb-4">
								<li>✓ Full access to all features</li>
								<li>✓ Unlimited medicines</li>
								<li>✓ Advanced reporting</li>
								<li>✓ Priority support</li>
							</ul>
							<button
								onclick={buySubscription}
								disabled={isProcessing}
								class={`w-full py-2 px-4 rounded-lg font-semibold transition ${
									isProcessing
										? 'bg-gray-600 text-gray-400 cursor-not-allowed'
										: 'bg-yellow-600 hover:bg-yellow-700 text-white'
								}`}
							>
								{isProcessing ? 'Processing...' : 'Upgrade to Premium'}
							</button>
						</div>
					</div>
				</div>
			</div>

			{#if successMessage}
				<div class="bg-green-900 border border-green-700 rounded-lg p-4">
					<p class="text-green-200">{successMessage}</p>
				</div>
			{/if}

			<!-- Store Details -->
			<div class="bg-gray-800 rounded-lg border border-gray-700 p-6">
				<h2 class="text-2xl font-bold mb-4">Store Information</h2>

				<div class="grid grid-cols-1 md:grid-cols-2 gap-6">
					<div>
						<p class="text-gray-400 text-sm mb-1">Phone Number</p>
						<p class="text-lg font-semibold">{tenant.phoneNumber}</p>
					</div>

					<div>
						<p class="text-gray-400 text-sm mb-1">Address</p>
						<p class="text-lg font-semibold">{tenant.address}</p>
					</div>

					<div>
						<p class="text-gray-400 text-sm mb-1">Account Created</p>
						<p class="text-lg font-semibold">{formatDate(tenant.createdAt)}</p>
					</div>

					<div>
						<p class="text-gray-400 text-sm mb-1">Status</p>
						<p class={`text-lg font-semibold ${tenant.isActive ? 'text-green-400' : 'text-red-400'}`}>
							{tenant.isActive ? 'Active' : 'Inactive'}
						</p>
					</div>
				</div>
			</div>
		{:else if !isLoading}
			<div class="bg-yellow-900 border border-yellow-700 rounded-lg p-4">
				<p class="text-yellow-200">Unable to load tenant information.</p>
			</div>
		{/if}
	</div>
</div>
