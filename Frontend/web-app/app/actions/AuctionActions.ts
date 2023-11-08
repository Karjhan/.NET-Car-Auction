'use server'

import { Auction, PageResult } from "@/types";

export default async function getData(query: string): Promise<PageResult<Auction>> {
    const result = await fetch(`http://localhost:6001/search${query}`)
    if (!result.ok) {
        throw new Error("Failed to fetch data!")
    }
    return result.json();
}