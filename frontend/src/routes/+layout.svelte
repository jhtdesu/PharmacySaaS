<script lang="ts">
	import './layout.css';
	import { page } from '$app/state';
	import favicon from '$lib/assets/favicon.svg';

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

<header class="flex gap-4 bg-green-900 p-2 ">
	<a class = "font-bold text-emerald-300 hover:text-emerald-500" href="/homepage" class:active={page.url.pathname.startsWith('/homepage')}>Home</a>
	<a class = "font-bold text-emerald-300 hover:text-emerald-500" href="/dashboard" class:active={page.url.pathname.startsWith('/dashboard')}>Dashboard</a>
	<a class = "font-bold text-emerald-300 hover:text-emerald-500" href="/medicines" class:active={page.url.pathname.startsWith('/medicines')}>Medicines</a>
	<a class = "font-bold text-emerald-300 hover:text-emerald-500" href="/medicineBatches" class:active={page.url.pathname.startsWith('/medicineBatches')}>Batches</a>
	<a class = "font-bold text-emerald-300 hover:text-emerald-500" href="/pos" class:active={page.url.pathname.startsWith('/pos')}>POS</a>
	<a class = "font-bold text-emerald-300 hover:text-emerald-500" href="/lowStock" class:active={page.url.pathname.startsWith('/lowStock')}>Low Stock</a>
	<a class = "font-bold text-emerald-300 hover:text-emerald-500" href="/auth" class:active={page.url.pathname.startsWith('/auth')}>Login</a>
</header>

<main class="page-content">
	{@render children()}
</main>
