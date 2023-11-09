'use server'

import { Auction, PageResult } from "@/types";
import { getTokenWorkaround } from "./AuthActions";

export default async function getData(query: string): Promise<PageResult<Auction>> {
    const result = await fetch(`http://localhost:6001/search${query}`)
    if (!result.ok) {
        throw new Error("Failed to fetch data!")
    }
    return result.json();
}

export async function updateAuctionTest() {
    const data = {
        mileage: Math.floor(Math.random() * 100000) + 1
    }

    const token = await getTokenWorkaround()

    const res = await fetch("http://localhost:6001/auctions/afbee524-5972-4075-8800-7d1f9d7b0a0c", {
        method: "PUT",
        headers: {
            'Content-Type': 'application/json',
            'Authorization': "Bearer " + token?.access_token
        },
        body: JSON.stringify(data)
    });

    if (!res.ok) {
        return {status: res.status, message: res.statusText}
    }
    return res.statusText
}