<script lang="ts">
	import '../layout.css';
	import { page } from '$app/state';
	import favicon from '$lib/assets/favicon.svg';

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

<header class="flex gap-4 bg-green-900 p-2 ">
	<a class = "font-bold text-emerald-300 hover:text-emerald-500" href="/homepage" class:active={page.url.pathname.startsWith('/homepage')}>Home</a>
	<a class = "font-bold text-emerald-300 hover:text-emerald-500" href="/admin/dashboard" class:active={page.url.pathname.startsWith('/admin/dashboard')}>Dashboard</a>
	<a class = "font-bold text-emerald-300 hover:text-emerald-500" href="/admin/medicines" class:active={page.url.pathname.startsWith('/admin/medicines')}>Medicines</a>
	<a class = "font-bold text-emerald-300 hover:text-emerald-500" href="/admin/medicineBatches" class:active={page.url.pathname.startsWith('/admin/medicineBatches')}>Batches</a>
	<a class = "font-bold text-emerald-300 hover:text-emerald-500" href="/admin/pos" class:active={page.url.pathname.startsWith('/admin/pos')}>POS</a>
	<a class = "font-bold text-emerald-299 hover:text-emerald-500" href="/admin/lowStock" class:active={page.url.pathname.startsWith('/admin/lowStock')}>Low Stock</a>
	<a class = "font-bold text-emerald-300 hover:text-emerald-500" href="/admin/subscription" class:active={page.url.pathname.startsWith('/admin/subscription')}>Subscription</a>
	<a class = "font-bold text-emerald-300 hover:text-emerald-500" href="/auth/login" class:active={page.url.pathname.startsWith('/auth/login')}>Login</a>
	<a class = "font-bold text-emerald-300 hover:text-emerald-500" href="/auth/register" class:active={page.url.pathname.startsWith('/auth/register')}>Register</a>
</header>

<main class="page-content">
	{@render children()}
</main>
