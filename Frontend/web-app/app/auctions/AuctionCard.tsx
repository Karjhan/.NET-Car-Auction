import React from 'react'
import Link from 'next/link'
import Image from 'next/image'
import CountdownTimer from './CoundowntTimer'
import CarImage from './CarImage'
import { Auction } from '@/types'
import CurrentBid from './CurrentBid'

type Props = {
    auction: Auction
}

const AuctionCard = (props: Props) => {
  return (
      <Link href={`/auctions/details/${props.auction.id}`} className='group'>
          <div className='w-full bg-gray-200 aspect-w-16 aspect-h-10 rounded-lg overflow-hidden'>
              <CarImage imageURL={props.auction.imageURL} />
              <div className='absolute top-44 left-2 w-36'>
                  <CountdownTimer auctionEnd={props.auction.auctionEnd} />
              </div>
              <div className='absolute top-3 left-52 w-36'>
                  <CurrentBid reservePrice={props.auction.reservePrice} amount={props.auction.currentHighBid} />
              </div>
          </div>
          <div className='flex justify-between items-center mt-4'>
              <h3 className='text-gray-700'>{props.auction.make} {props.auction.model}</h3>
              <p className='font-semibold text-sm'>{props.auction.year}</p>
          </div>

      </Link>
  )
}

export default AuctionCard
