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
			const userName = localStorage.getItem('user_full_name') || 'Customer';

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

<div class="min-h-screen bg-gray-900 p-6 text-white">
	<div class="mx-auto max-w-4xl">
		<h1 class="mb-8 text-4xl font-bold">Subscription Management</h1>

		{#if isLoading}
			<div class="py-12 text-center">
				<p class="text-gray-400">Loading subscription information...</p>
			</div>
		{:else if errorMessage}
			<div class="mb-6 rounded-lg border border-red-700 bg-red-900 p-4">
				<p class="text-red-200">{errorMessage}</p>
			</div>
		{/if}

		{#if tenant}
			<div class="mb-8 grid grid-cols-1 gap-6 md:grid-cols-2">
				<!-- Current Subscription Info -->
				<div class="rounded-lg border border-gray-700 bg-gray-800 p-6">
					<h2 class="mb-4 text-2xl font-bold">Current Subscription</h2>

					<div class="space-y-4">
						<div>
							<p class="mb-1 text-sm text-gray-400">Store Name</p>
							<p class="text-lg font-semibold">{tenant.storeName}</p>
						</div>

						<div>
							<p class="mb-1 text-sm text-gray-400">Subscription Plan</p>
							<span
								class={`inline-block rounded-full px-3 py-1 text-sm font-semibold ${getSubscriptionBadgeColor(tenant.subscription)}`}
							>
								{tenant.subscription}
							</span>
						</div>

						<div>
							<p class="mb-1 text-sm text-gray-400">Status</p>
							<span
								class={`inline-block rounded-full px-3 py-1 text-sm font-semibold ${getStatusBadgeColor(tenant.subscriptionStatus)}`}
							>
								{tenant.subscriptionStatus}
							</span>
						</div>

						<div>
							<p class="mb-1 text-sm text-gray-400">Expiry Date</p>
							<p class="text-lg font-semibold">
								{formatDate(tenant.subscriptionExpiry)}
							</p>
							{#if isSubscriptionExpired()}
								<p class="mt-1 text-sm text-red-400">⚠️ Subscription expired</p>
							{:else}
								<p class="mt-1 text-sm text-green-400">✓ {getDaysUntilExpiry()} days remaining</p>
							{/if}
						</div>
					</div>
				</div>

				<!-- Subscription Plans -->
				<div class="rounded-lg border border-gray-700 bg-gray-800 p-6">
					<h2 class="mb-4 text-2xl font-bold">Available Plans</h2>

					<div class="space-y-4">
						<div class="rounded-lg border border-yellow-600 bg-gray-700 p-4">
							<div class="mb-2 flex items-start justify-between">
								<h3 class="text-xl font-bold text-yellow-400">Premium Plan</h3>
								<span class="text-2xl font-bold">500,000₫</span>
							</div>
							<p class="mb-3 text-sm text-gray-300">Per month</p>
							<ul class="mb-4 space-y-2 text-sm text-gray-300">
								<li>✓ Full access to all features</li>
								<li>✓ Unlimited medicines</li>
								<li>✓ Advanced reporting</li>
								<li>✓ Priority support</li>
							</ul>
							<button
								onclick={buySubscription}
								disabled={isProcessing}
								class={`w-full rounded-lg px-4 py-2 font-semibold transition ${
									isProcessing
										? 'cursor-not-allowed bg-gray-600 text-gray-400'
										: 'bg-yellow-600 text-white hover:bg-yellow-700'
								}`}
							>
								{isProcessing ? 'Processing...' : 'Upgrade to Premium'}
							</button>
						</div>
					</div>
				</div>
			</div>

			{#if successMessage}
				<div class="rounded-lg border border-green-700 bg-green-900 p-4">
					<p class="text-green-200">{successMessage}</p>
				</div>
			{/if}

			<!-- Store Details -->
			<div class="rounded-lg border border-gray-700 bg-gray-800 p-6">
				<h2 class="mb-4 text-2xl font-bold">Store Information</h2>

				<div class="grid grid-cols-1 gap-6 md:grid-cols-2">
					<div>
						<p class="mb-1 text-sm text-gray-400">Phone Number</p>
						<p class="text-lg font-semibold">{tenant.phoneNumber}</p>
					</div>

					<div>
						<p class="mb-1 text-sm text-gray-400">Address</p>
						<p class="text-lg font-semibold">{tenant.address}</p>
					</div>

					<div>
						<p class="mb-1 text-sm text-gray-400">Account Created</p>
						<p class="text-lg font-semibold">{formatDate(tenant.createdAt)}</p>
					</div>

					<div>
						<p class="mb-1 text-sm text-gray-400">Status</p>
						<p
							class={`text-lg font-semibold ${tenant.isActive ? 'text-green-400' : 'text-red-400'}`}
						>
							{tenant.isActive ? 'Active' : 'Inactive'}
						</p>
					</div>
				</div>
			</div>
		{:else if !isLoading}
			<div class="rounded-lg border border-yellow-700 bg-yellow-900 p-4">
				<p class="text-yellow-200">Unable to load tenant information.</p>
			</div>
		{/if}
	</div>
</div>
